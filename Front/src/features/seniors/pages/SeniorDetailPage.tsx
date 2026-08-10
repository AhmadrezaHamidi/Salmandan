import { useState } from "react";
import { useParams } from "react-router-dom";
import { Button, Card, CardTitle, Badge } from "@/components/ui";
import { Input, Select } from "@/components/ui/Input";
import { Spinner, ErrorState } from "@/components/ui/Spinner";
import {
  useSenior, useAddCondition, useAddMedication, useAddAllergy,
  useAddEmergencyContact, useAddFamilyMember, useSetDependencyLevel,
} from "@/features/seniors/api";
import { dependencyLevelLabels, type FamilyRelation, type FamilyAccessLevel } from "@/types/enums";
import { extractErrorMessage } from "@/lib/apiClient";

function Section({ title, action, children }: { title: string; action?: React.ReactNode; children: React.ReactNode }) {
  return (
    <Card className="mb-4">
      <div className="mb-3 flex items-center justify-between">
        <CardTitle className="mb-0">{title}</CardTitle>
        {action}
      </div>
      {children}
    </Card>
  );
}

function AddToggle({ label, children }: { label: string; children: (close: () => void) => React.ReactNode }) {
  const [open, setOpen] = useState(false);
  if (!open) return <Button variant="ghost" onClick={() => setOpen(true)}>+ {label}</Button>;
  return <div className="mt-3 space-y-2 border-t border-gray-100 pt-3">{children(() => setOpen(false))}</div>;
}

export function SeniorDetailPage() {
  const { id } = useParams();
  const seniorId = Number(id);
  const { data: senior, isLoading, error } = useSenior(seniorId);
  const addCondition = useAddCondition(seniorId);
  const addMedication = useAddMedication(seniorId);
  const addAllergy = useAddAllergy(seniorId);
  const addContact = useAddEmergencyContact(seniorId);
  const addFamily = useAddFamilyMember(seniorId);
  const setDependency = useSetDependencyLevel(seniorId);

  if (isLoading) return <Spinner />;
  if (error) return <ErrorState message={extractErrorMessage(error)} />;
  if (!senior) return null;

  return (
    <div className="mx-auto max-w-3xl">
      <div className="mb-5 flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-900">{senior.fullName}</h1>
          <p className="text-sm text-gray-500">{senior.city} — {senior.age} سال</p>
        </div>
        <Select
          value={senior.dependencyLevel}
          onChange={(e) => setDependency.mutate(e.target.value as never)}
          options={Object.entries(dependencyLevelLabels).map(([value, label]) => ({ value, label }))}
        />
      </div>

      <Section title="شرایط پزشکی">
        <ul className="space-y-1 text-sm">
          {senior.conditions.map((c) => (
            <li key={c.id} className="flex items-center gap-2">
              <span>{c.title}</span>
              {c.isChronic && <Badge color="amber">مزمن</Badge>}
            </li>
          ))}
        </ul>
        <AddToggle label="افزودن شرایط پزشکی">
          {(close) => (
            <QuickForm
              fields={[{ name: "title", label: "عنوان" }, { name: "description", label: "توضیح (اختیاری)" }]}
              onSubmit={async (v) => { await addCondition.mutateAsync({ ...v, isChronic: false } as never); close(); }}
            />
          )}
        </AddToggle>
      </Section>

      <Section title="داروها">
        <ul className="space-y-1 text-sm">
          {senior.medications.map((m) => (
            <li key={m.id}>{m.name} — {m.dosage} {!m.isActive && <Badge color="gray">قطع‌شده</Badge>}</li>
          ))}
        </ul>
        <AddToggle label="افزودن دارو">
          {(close) => (
            <QuickForm
              fields={[
                { name: "name", label: "نام دارو" }, { name: "dosage", label: "دوز" },
                { name: "startDate", label: "تاریخ شروع", type: "date" },
              ]}
              onSubmit={async (v) => { await addMedication.mutateAsync({ ...v, frequency: "OnceDaily" } as never); close(); }}
            />
          )}
        </AddToggle>
      </Section>

      <Section title="حساسیت‌ها">
        <ul className="space-y-1 text-sm">
          {senior.allergies.map((a) => <li key={a.id}>{a.title}{a.reaction ? ` — ${a.reaction}` : ""}</li>)}
        </ul>
        <AddToggle label="افزودن حساسیت">
          {(close) => (
            <QuickForm
              fields={[{ name: "title", label: "عنوان" }, { name: "reaction", label: "واکنش (اختیاری)" }]}
              onSubmit={async (v) => { await addAllergy.mutateAsync(v as never); close(); }}
            />
          )}
        </AddToggle>
      </Section>

      <Section title="مخاطبین اضطراری">
        <ul className="space-y-1 text-sm">
          {senior.emergencyContacts.map((c) => <li key={c.id}>{c.fullName} — {c.phoneNumber}</li>)}
        </ul>
        <AddToggle label="افزودن مخاطب اضطراری">
          {(close) => (
            <QuickForm
              fields={[{ name: "fullName", label: "نام" }, { name: "phoneNumber", label: "شماره تماس", dir: "ltr" }]}
              onSubmit={async (v) => { await addContact.mutateAsync({ ...v, relation: "Other", priority: 1 } as never); close(); }}
            />
          )}
        </AddToggle>
      </Section>

      <Section title="اعضای خانواده">
        <ul className="space-y-1 text-sm">
          {senior.familyMembers.map((f) => (
            <li key={f.userId} className="flex items-center gap-2">
              <span>{f.displayName}</span>
              <Badge>{f.accessLevel}</Badge>
            </li>
          ))}
        </ul>
        <AddToggle label="افزودن عضو خانواده">
          {(close) => (
            <QuickForm
              fields={[{ name: "phoneNumber", label: "شماره موبایل", dir: "ltr" }]}
              onSubmit={async (v) => {
                await addFamily.mutateAsync({
                  ...v, relation: "Other" as FamilyRelation, accessLevel: "Viewer" as FamilyAccessLevel,
                } as never);
                close();
              }}
            />
          )}
        </AddToggle>
      </Section>
    </div>
  );
}

function QuickForm({
  fields, onSubmit,
}: {
  fields: { name: string; label: string; type?: string; dir?: string }[];
  onSubmit: (values: Record<string, string>) => Promise<void>;
}) {
  const [values, setValues] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  return (
    <form
      className="space-y-2"
      onSubmit={async (e) => {
        e.preventDefault();
        setSubmitting(true);
        try { await onSubmit(values); } finally { setSubmitting(false); }
      }}
    >
      {fields.map((f) => (
        <Input
          key={f.name}
          label={f.label}
          type={f.type}
          dir={f.dir as never}
          value={values[f.name] ?? ""}
          onChange={(e) => setValues((v) => ({ ...v, [f.name]: e.target.value }))}
        />
      ))}
      <Button type="submit" loading={submitting}>ثبت</Button>
    </form>
  );
}
