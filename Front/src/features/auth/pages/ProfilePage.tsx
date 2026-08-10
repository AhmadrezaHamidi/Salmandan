import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Button, Card, CardTitle } from "@/components/ui";
import { Input } from "@/components/ui/Input";
import { ErrorState } from "@/components/ui/Spinner";
import { useUpdateProfile } from "@/features/auth/api";
import { useAuthStore } from "@/store/authStore";
import { extractErrorMessage } from "@/lib/apiClient";

const schema = z.object({
  firstName: z.string().min(1, "نام الزامی است").optional(),
  lastName: z.string().min(1, "نام خانوادگی الزامی است").optional(),
  nationalId: z.string().regex(/^\d{10}$/, "کد ملی باید ۱۰ رقم باشد").optional().or(z.literal("")),
});

export function ProfilePage() {
  const updateDisplayName = useAuthStore((s) => s.updateDisplayName);
  const mutation = useUpdateProfile();
  const { register, handleSubmit, formState } = useForm<z.infer<typeof schema>>({ resolver: zodResolver(schema) });

  async function onSubmit(values: z.infer<typeof schema>) {
    await mutation.mutateAsync(values);
    if (values.firstName && values.lastName) updateDisplayName(`${values.firstName} ${values.lastName}`);
  }

  return (
    <div className="mx-auto max-w-md">
      <Card>
        <CardTitle>تکمیل پروفایل</CardTitle>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input label="نام" {...register("firstName")} error={formState.errors.firstName?.message} />
          <Input label="نام خانوادگی" {...register("lastName")} error={formState.errors.lastName?.message} />
          <Input label="کد ملی" dir="ltr" {...register("nationalId")} error={formState.errors.nationalId?.message} />
          {mutation.isError && <ErrorState message={extractErrorMessage(mutation.error)} />}
          {mutation.isSuccess && <p className="text-sm text-green-600">پروفایل با موفقیت به‌روزرسانی شد</p>}
          <Button type="submit" loading={mutation.isPending}>
            ذخیره
          </Button>
        </form>
      </Card>
    </div>
  );
}
