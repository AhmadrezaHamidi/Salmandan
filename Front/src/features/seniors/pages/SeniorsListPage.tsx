import { useNavigate } from "react-router-dom";
import { Button, Card, Badge } from "@/components/ui";
import { Spinner, ErrorState, EmptyState } from "@/components/ui/Spinner";
import { useMySeniors } from "@/features/seniors/api";
import { dependencyLevelLabels } from "@/types/enums";
import { extractErrorMessage } from "@/lib/apiClient";

export function SeniorsListPage() {
  const { data, isLoading, error } = useMySeniors();
  const navigate = useNavigate();

  return (
    <div>
      <div className="mb-5 flex items-center justify-between">
        <h1 className="text-xl font-bold text-gray-900">سالمندان من</h1>
        <Button onClick={() => navigate("/seniors/new")}>افزودن سالمند</Button>
      </div>

      {isLoading && <Spinner />}
      {error && <ErrorState message={extractErrorMessage(error)} />}
      {data && data.length === 0 && <EmptyState message="هنوز سالمندی ثبت نکرده‌اید" />}

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {data?.map((s) => (
          <Card key={s.id} className="cursor-pointer hover:border-brand-300" onClick={() => navigate(`/seniors/${s.id}`)}>
            <div className="mb-2 flex items-start justify-between">
              <span className="font-semibold text-gray-900">{s.fullName}</span>
              <Badge>{dependencyLevelLabels[s.dependencyLevel]}</Badge>
            </div>
            <div className="text-sm text-gray-500">
              {s.city}
              {s.district ? ` — ${s.district}` : ""}
            </div>
            <div className="mt-1 text-sm text-gray-400">{s.age} سال</div>
          </Card>
        ))}
      </div>
    </div>
  );
}
