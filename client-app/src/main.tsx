import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { createBrowserRouter, RouterProvider, Navigate } from "react-router-dom";
import App from "./app/layout/App.tsx";
import ActivityDashboard from "./features/activities/dashboard/ActivityDashboard.tsx";
import ActivityDetails from "./features/activities/details/ActivityDetails.tsx";
import ActivityForm from "./features/activities/form/ActivityForm.tsx";
import LoginForm from "./features/user/LoginForm.tsx";
import RegisterForm from "./features/user/RegisterForm.tsx";
import "semantic-ui-css/semantic.min.css";
import "./app/layout/styles.css";
import { store, StoreContext } from "./app/stores/store.ts";

const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      { index: true, element: <Navigate to="/activities" replace /> },
      { path: "activities", element: <ActivityDashboard /> },
      { path: "activities/create", element: <ActivityForm key="create" /> },
      { path: "activities/:id", element: <ActivityDetails /> },
      { path: "activities/:id/edit", element: <ActivityForm key="edit" /> },
      { path: "login", element: <LoginForm /> },
      { path: "register", element: <RegisterForm /> },
    ],
  },
]);

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <StoreContext.Provider value={store}>
      <RouterProvider router={router} />
    </StoreContext.Provider>
  </StrictMode>
);
