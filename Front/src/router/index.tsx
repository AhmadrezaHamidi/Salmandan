import { createBrowserRouter, Navigate } from "react-router-dom";
import { AuthLayout } from "@/layouts/AuthLayout";
import { AppLayout } from "@/layouts/AppLayout";
import { ProtectedRoute } from "@/router/ProtectedRoute";
import { LoginPage } from "@/features/auth/pages/LoginPage";
import { ProfilePage } from "@/features/auth/pages/ProfilePage";
import { DashboardPage } from "@/features/dashboard/DashboardPage";
import { SeniorsListPage } from "@/features/seniors/pages/SeniorsListPage";
import { SeniorDetailPage } from "@/features/seniors/pages/SeniorDetailPage";
import { SeniorCreatePage } from "@/features/seniors/pages/SeniorCreatePage";
import { ShiftsListPage } from "@/features/shifts/pages/ShiftsListPage";
import { ShiftDetailPage } from "@/features/shifts/pages/ShiftDetailPage";
import { ShiftRequestPage } from "@/features/shifts/pages/ShiftRequestPage";
import { AgenciesListPage } from "@/features/agencies/pages/AgenciesListPage";
import { AgencyDetailPage } from "@/features/agencies/pages/AgencyDetailPage";
import { CaregiversListPage } from "@/features/caregivers/pages/CaregiversListPage";
import { CaregiverDetailPage } from "@/features/caregivers/pages/CaregiverDetailPage";
import { ReportsListPage } from "@/features/reports/pages/ReportsListPage";
import { ReportDetailPage } from "@/features/reports/pages/ReportDetailPage";
import { OrdersListPage } from "@/features/orders/pages/OrdersListPage";
import { PayoutsListPage } from "@/features/payouts/pages/PayoutsListPage";

export const router = createBrowserRouter([
  {
    element: <AuthLayout />,
    children: [{ path: "/login", element: <LoginPage /> }],
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { path: "/", element: <DashboardPage /> },
          { path: "/profile", element: <ProfilePage /> },
          { path: "/seniors", element: <SeniorsListPage /> },
          { path: "/seniors/new", element: <SeniorCreatePage /> },
          { path: "/seniors/:id", element: <SeniorDetailPage /> },
          { path: "/shifts", element: <ShiftsListPage /> },
          { path: "/shifts/new", element: <ShiftRequestPage /> },
          { path: "/shifts/:id", element: <ShiftDetailPage /> },
          { path: "/agencies", element: <AgenciesListPage /> },
          { path: "/agencies/:id", element: <AgencyDetailPage /> },
          { path: "/caregivers", element: <CaregiversListPage /> },
          { path: "/caregivers/:id", element: <CaregiverDetailPage /> },
          { path: "/reports", element: <ReportsListPage /> },
          { path: "/reports/:id", element: <ReportDetailPage /> },
          { path: "/orders", element: <OrdersListPage /> },
          { path: "/payouts", element: <PayoutsListPage /> },
        ],
      },
    ],
  },
  { path: "*", element: <Navigate to="/" replace /> },
]);
