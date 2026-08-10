import { useState } from "react";
import { useParams } from "react-router-dom";
import { Button, Card, CardTitle, Badge } from "@/components/ui";
import { Input } from "@/components/ui/Input";
import { Spinner, ErrorState } from "@/components/ui/Spinner";
import {
  useShift, useAssignCaregiver, useCheckInShift, useCheckOutShift,
  useCompleteShift, useCancelShift, useMarkNoShow,
} from "@/features/shifts/api";
import { useAuthStore } from "@/store/authStore";
import { useCreateOrder } from "@/features/orders/api";
import { shiftStatusLabels, shiftStatusColor, careServiceTypeLabels, shiftKindLabels } from "@/types/enums";
import { extractErrorMessage } from "@/lib/apiClient";

export function ShiftDetailPage() {
  const { id } = useParams();
  const shiftId = Number(id);
  const role = useAuthStore((s) => s.session?.role);
  const { data: shift, isLoading, error } = useShift(shiftId);

  const assign = useAssignCaregiver(shiftId);
  const checkIn = useCheckInShift(shiftId);
  const checkOut = useCheckOutShift(shiftId);
  const complete = useCompleteShift(shiftId);
  const cancel = useCancelShift(shiftId);
  const noShow = useMarkNoShow(shiftId);
  const createOrder = useCreateOrder();

  const [caregiverId, setCaregiverId] = useState("");
  const [cancelReason, setCancelReason] = useState("");
  const anyMutation = [assign, checkIn, checkOut, complete, cancel, noShow];
  const mutationError = anyMutation.find((m) => m.isError)?.error;

  if (isLoading) return <Spinner />;
  if (error) return <ErrorState message={extractErrorMessage(error)} />;
  if (!shift) return null;

  function doCheckIn() {
    navigator.geolocation.getCurrentPosition(
      (pos) => checkIn.mutate({ latitude: pos.coords.latitude, longitude: pos.coords.longitude }),
      () => alert("دسترسی به موقعیت مکانی امکان‌پذیر نیست")
    );
  }

  return (
    <div className="mx-auto max-w-xl">
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-900">شیفت #{shift.id}</h1>
        <Badge color={shiftStatusColor[shift.status]}>{shiftStatusLabels[shift.status]}</Badge>
      </div>

      <Card className="mb-4 space-y-2 text-sm">
        <div>تاریخ: {shift.date}</div>
        <div>ساعت: {shift.startTime} - {shift.endTime}</div>
        <div>نوع خدمت: {careServiceTypeLabels[shift.serviceType]}</div>
        <div>نوع شیفت: {shiftKindLabels[shift.shiftKind]}</div>
        <div>مبلغ: {shift.price.toLocaleString("fa-IR")} تومان</div>
        {shift.familyNote && <div>یادداشت خانواده: {shift.familyNote}</div>}
        {shift.cancelReason && <div className="text-red-600">دلیل لغو: {shift.cancelReason}</div>}
      </Card>

      {mutationError !== undefined && <ErrorState message={extractErrorMessage(mutationError)} />}

      <div className="space-y-4">
        {(role === "Admin" || role === "AgencyManager") && shift.status === "Requested" && (
          <Card>
            <CardTitle>تخصیص پرستار</CardTitle>
            <div className="flex gap-2">
              <Input placeholder="شناسه پرستار" value={caregiverId} onChange={(e) => setCaregiverId(e.target.value)} />
              <Button loading={assign.isPending} onClick={() => assign.mutate({ caregiverId: Number(caregiverId) })}>
                تخصیص
              </Button>
            </div>
          </Card>
        )}

        {(role === "Admin" || role === "Caregiver") && shift.status === "Assigned" && (
          <Button loading={checkIn.isPending} onClick={doCheckIn}>ثبت حضور (Check-in)</Button>
        )}

        {(role === "Admin" || role === "Caregiver") && shift.status === "CheckedIn" && (
          <Button loading={checkOut.isPending} onClick={() => checkOut.mutate()}>پایان شیفت (Check-out)</Button>
        )}

        {(role === "Admin" || role === "AgencyManager") && shift.status === "CheckedOut" && (
          <Button loading={complete.isPending} onClick={() => complete.mutate()}>تکمیل شیفت</Button>
        )}

        {(role === "Admin" || role === "Family") && shift.status === "Completed" && (
          <Button
            loading={createOrder.isPending}
            onClick={async () => {
              const res = await createOrder.mutateAsync(shift.id);
              window.location.href = res.paymentUrl;
            }}
          >
            پرداخت هزینه شیفت
          </Button>
        )}

        {(role === "Admin" || role === "AgencyManager") && shift.status === "Assigned" && (
          <Button variant="secondary" loading={noShow.isPending} onClick={() => noShow.mutate()}>ثبت عدم‌حضور پرستار</Button>
        )}

        {["Requested", "Assigned"].includes(shift.status) && (
          <Card>
            <CardTitle>لغو شیفت</CardTitle>
            <div className="flex gap-2">
              <Input placeholder="دلیل لغو" value={cancelReason} onChange={(e) => setCancelReason(e.target.value)} />
              <Button
                variant="danger"
                loading={cancel.isPending}
                onClick={() =>
                  cancel.mutate({
                    cancelledBy: role === "AgencyManager" ? "Agency" : role === "Admin" ? "Platform" : "Family",
                    reason: cancelReason,
                  })
                }
              >
                لغو شیفت
              </Button>
            </div>
          </Card>
        )}
      </div>
    </div>
  );
}
