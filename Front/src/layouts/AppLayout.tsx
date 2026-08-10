import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { useAuthStore } from "@/store/authStore";
import { roleLabels } from "@/types/enums";
import { clsx } from "@/lib/clsx";
import { logout } from "@/features/auth/api";

const navByRole: Record<string, { to: string; label: string }[]> = {
  common: [{ to: "/", label: "داشبورد" }],
  Admin: [
    { to: "/seniors", label: "سالمندان" },
    { to: "/agencies", label: "آژانس‌ها" },
    { to: "/caregivers", label: "پرستاران" },
    { to: "/shifts", label: "شیفت‌ها" },
    { to: "/reports", label: "گزارش‌ها" },
    { to: "/orders", label: "سفارش‌ها" },
    { to: "/payouts", label: "تسویه حقوق" },
  ],
  Family: [
    { to: "/seniors", label: "سالمندان من" },
    { to: "/shifts", label: "شیفت‌ها" },
    { to: "/reports", label: "گزارش‌ها" },
    { to: "/orders", label: "پرداخت‌ها" },
  ],
  AgencyManager: [
    { to: "/agencies", label: "آژانس من" },
    { to: "/caregivers", label: "پرستاران" },
    { to: "/shifts", label: "شیفت‌ها" },
    { to: "/payouts", label: "تسویه حقوق" },
  ],
  Caregiver: [{ to: "/shifts", label: "شیفت‌های من" }],
};

export function AppLayout() {
  const session = useAuthStore((s) => s.session);
  const clear = useAuthStore((s) => s.clear);
  const navigate = useNavigate();

  if (!session) return null;
  const links = [...navByRole.common, ...(navByRole[session.role] ?? [])];

  async function handleLogout() {
    await logout();
    clear();
    navigate("/login");
  }

  return (
    <div className="flex min-h-screen">
      <aside className="flex w-60 shrink-0 flex-col border-l border-gray-200 bg-white">
        <div className="border-b border-gray-100 px-5 py-5">
          <div className="text-lg font-bold text-brand-800">سالمندان</div>
          <div className="mt-1 text-xs text-gray-400">{roleLabels[session.role]}</div>
        </div>
        <nav className="flex-1 space-y-1 px-3 py-4">
          {links.map((l) => (
            <NavLink
              key={l.to}
              to={l.to}
              end={l.to === "/"}
              className={({ isActive }) =>
                clsx(
                  "block rounded-lg px-3 py-2 text-sm font-medium",
                  isActive ? "bg-brand-50 text-brand-700" : "text-gray-600 hover:bg-gray-50"
                )
              }
            >
              {l.label}
            </NavLink>
          ))}
        </nav>
        <div className="border-t border-gray-100 p-3">
          <div className="mb-2 truncate px-2 text-sm text-gray-700">{session.displayName}</div>
          <button onClick={handleLogout} className="w-full rounded-lg px-3 py-2 text-right text-sm text-red-600 hover:bg-red-50">
            خروج
          </button>
        </div>
      </aside>
      <main className="flex-1 overflow-y-auto p-6">
        <Outlet />
      </main>
    </div>
  );
}
