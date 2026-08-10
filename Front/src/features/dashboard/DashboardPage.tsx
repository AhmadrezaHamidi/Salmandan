import { Card } from "@/components/ui";
import { useAuthStore } from "@/store/authStore";
import { roleLabels } from "@/types/enums";
import { useMySeniors } from "@/features/seniors/api";

export function DashboardPage() {
  const session = useAuthStore((s) => s.session);
  const { data: seniors } = useMySeniors();

  return (
    <div>
      <h1 className="mb-1 text-xl font-bold text-gray-900">سلام {session?.displayName} 👋</h1>
      <p className="mb-6 text-sm text-gray-500">نقش شما: {session && roleLabels[session.role]}</p>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card>
          <div className="text-2xl font-bold text-brand-700">{seniors?.length ?? "—"}</div>
          <div className="text-sm text-gray-500">سالمند تحت مراقبت</div>
        </Card>
      </div>
    </div>
  );
}
