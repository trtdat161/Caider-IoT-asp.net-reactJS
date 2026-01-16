import { StrictMode } from "react";
import { createRoot } from "react-dom/client";

import { Dashboard } from "./Dashboard.jsx";
import { BrowserRouter, Routes, Route } from "react-router-dom"; // bộ 3 của react-router-dom
import { DashboardManage } from "./DashboardManage.jsx";
import { ManagehardWare } from "./ManagehardWare.jsx";
import { Microcontroller } from "./Microcontroller.jsx";
import { Expansiveboard } from "./Expansiveboard.jsx";
import { Servo } from "./Servo.jsx";
import { Motor } from "./Motor.jsx";
import { MicrocontrollerForm } from "./crud/MicrocontrollerForm.jsx";
import { ExpansiveForm } from "./crud/ExpansiveForm.jsx";
import { ServoForm } from "./crud/ServoForm.jsx";
import { MotorForm } from "./crud/MotorForm.jsx";
import { Login } from "./Login.jsx";

createRoot(document.getElementById("root")).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        {/* còn cái login */}
        <Route path="/" element={<Login />} />
        <Route path="/dashboard" element={<Dashboard />} />
        {/* ----- Outlet dùng phải bọc mấy thằng con bên trong như này ----- */}
        <Route path="/manage" element={<DashboardManage />}>
          <Route index element={<ManagehardWare />} />
          <Route path="hardware" element={<ManagehardWare />} />
          <Route path="microcontroller" element={<Microcontroller />} />
          {/* crud microcontroller */}
          <Route
            path="add-or-up-microcontroller"
            element={<MicrocontrollerForm />}
          />
          <Route
            path="add-or-up-microcontroller/:id"
            element={<MicrocontrollerForm />}
          />

          <Route path="expansiveboard" element={<Expansiveboard />} />
          {/* crud expansive form */}
          <Route path="add-or-up-expansive" element={<ExpansiveForm />} />
          <Route path="add-or-up-expansive/:id" element={<ExpansiveForm />} />

          <Route path="servo" element={<Servo />} />
          {/* crud servo form */}
          <Route path="add-or-up-servo" element={<ServoForm />} />
          <Route path="add-or-up-servo/:id" element={<ServoForm />} />

          <Route path="motor" element={<Motor />} />
          {/* crud motor form */}
          <Route path="add-or-up-motor" element={<MotorForm />} />
          <Route path="add-or-up-motor/:id" element={<MotorForm />} />
        </Route>
      </Routes>
    </BrowserRouter>
  </StrictMode>
);
