import {BrowserRouter, Routes, Route} from "react-router-dom"
import {QueryClient, QueryClientProvider} from "@tanstack/react-query"
import {AppLayout} from "@/components/layout/AppLayout"
import { HomePage } from '@/pages/HomePage'
import { LoginPage } from '@/pages/LoginPage'
import { RegisterPage } from '@/pages/RegisterPage'
import { ProfilePage } from '@/pages/ProfilePage'
import { SettingsPage } from '@/pages/SettingsPage'
import { AnalyzePage } from '@/pages/AnalyzePage'
import { MyAnalysesPage } from '@/pages/MyAnalysesPage'
import { AnalysisDetailPage } from '@/pages/AnalysisDetailPage'
import { FeedPage } from '@/pages/FeedPage'
import { PublicProfilePage } from '@/pages/PublicProfilePage'
import { PlansPage } from '@/pages/PlansPage'
import { DashboardPage } from '@/pages/DashboardPage'
import { SupportPage } from '@/pages/SupportPage'
import { NotFoundPage } from '@/pages/NotFoundPage'
import { ProtectedRoute } from '@/components/auth/ProtectedRoute'
import { Toaster } from '@/components/ui/sonner'


const queryClient = new QueryClient()

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Toaster richColors position="top-right" />
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* All other routes wrap in AppLayout (navbar, etc.) */}
          <Route element={<AppLayout />}>
            {/* Public */}
            <Route path="/" element={<HomePage />} />

            {/* Public */}
            <Route path="/feed" element={<FeedPage />} />
            <Route path="/analyses/:id" element={<AnalysisDetailPage />} />
            <Route path="/users/:id" element={<PublicProfilePage />} />

            {/* Auth-required — gated by ProtectedRoute */}
            <Route element={<ProtectedRoute />}>
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/profile" element={<ProfilePage />} />
              <Route path="/settings" element={<SettingsPage />} />
              <Route path="/analyze" element={<AnalyzePage />} />
              <Route path="/analyses" element={<MyAnalysesPage />} />
              <Route path="/plans" element={<PlansPage />} />
              <Route path="/support" element={<SupportPage />} />
            </Route>

            {/* 404 — must be last, inside AppLayout so the navbar still shows */}
            <Route path="*" element={<NotFoundPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}





export default App
