// import { BASE_URL } from "./BaseUrl";
import React, { useEffect, useState } from "react";
import CaiderScan from "./CaiderScan";
import "./css/dashboard.css";
import { useNavigate } from "react-router-dom";

export function Dashboard() {
  const [expansive, setExpansive] = useState("");
  const [micro, setMicro] = useState("");
  const [motor, setMotor] = useState("");
  const [servo, setServo] = useState("");
  const [connect, setConnect] = useState(false);
  // chuyển trang
  const navigate = useNavigate();
  const manageHardware = () => {
    navigate("/manage");
  };

  // render ra các phần cuwsg robot
  const data = async () => {
    try {
      const [responseExp, responseMicro, responseServos, responseMotors] =
        await Promise.all([
          fetch("/api/expansiveboards"),
          fetch("/api/microcontrollers"),
          fetch("/api/servos"),
          fetch("/api/motors"),
        ]);
      const [dataExp, dataMicro, dataServos, dataMotors] = await Promise.all([
        // phải đúng thứ tự với fetch đẻ tránh bị data đảo ngược
        responseExp.json(),
        responseMicro.json(),
        responseServos.json(),
        responseMotors.json(),
      ]);
      // những lúc ko ra data ở json ra là biết để còn truy cập đc
      setExpansive(dataExp.items[0].name || "no data");
      setMicro(dataMicro.items[0].name || "no data");
      setMotor(dataMotors.items[0].name || "no data");
      setServo(dataServos.items[0].name || "no data");
    } catch (error) {
      console.log("lỗi: " + error);
    }
  };

  const connectCaider = async () => {
    try {
      const response = await fetch("/api/mqtt/connect", {
        method: "POST",
        headers: { "Content-type": "application/json" },
        body: JSON.stringify({ functionName: "connect" }),
      });
      const data = await response.json();
      if (data.success) {
        setConnect(true);
      } else {
        setConnect(false);
      }
    } catch (error) {
      console.log("lỗi: " + error.message);
      setConnect(false);
    }
  };
  const handleWaveHello = async () => {
    try {
      const response = await fetch("/api/mqtt/command", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ functionName: "wave" }),
      });
      const data = await response.json();
      if (data.success) {
        alert("Cader thực hiện vẫy tay");
      } else {
        alert("Gửi lệnh thất bại!");
      }
    } catch (error) {
      alert(`Lỗi khi gửi: ${error.message}`);
    }
  };
  const stopWaveHello = async () => {
    try {
      const response = await fetch("/api/mqtt/stop", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
      });
      const data = await response.json();
      if (data.success) {
        alert("stop caider thành công");
      } else {
        alert("stop không thành công!");
      }
    } catch (error) {
      alert(`Lỗi khi gửi: ${error.message}`);
    }
  };
  useEffect(() => {
    data();
  }, []);

  return (
    <>
      <div className="container-fluid min-vh-100 d-flex flex-column">
        <div className="flex-grow-1 d-flex flex-column">
          <header className="row">
            <div className="d-flex justify-content-between">
              <div className="status-caider p-2">
                <div>Caider Control Dashboard</div>
                <small>Robot Management System</small>
              </div>
              <div>
                <nav className="p-3">
                  <button className="gear-btn" onClick={manageHardware}>
                    <span className="me-2">caider manager</span>
                    <i className="bi bi-gear-wide-connected"></i>
                  </button>
                </nav>
              </div>
            </div>
          </header>

          <div className="row flex-grow-1">
            <div className="col-md-12">
              <header>
                <h2 className="title text-center my-3">caider welcome you</h2>
              </header>
            </div>
          </div>

          {/* row 2 */}
          <main className="row flex-grow-1">
            <aside className="col-md-3 display-left">
              <div className="infomation">
                <div className="info1">
                  <div>robot name: Caider</div>
                  <div>protocol: MQTT</div>
                  <div>status: {connect ? "Connected" : "Disconnected"}</div>
                </div>
                {/* info2 nhét HumanScan vào đây */}
                <div className="info2 p-2">
                  <CaiderScan />
                </div>
              </div>
            </aside>

            <div className="col-md-6 btn-main">
              <div>
                <div className="d-flex justify-content-center my-3 btn-connect">
                  <button onClick={connectCaider}>Connect with Caider</button>
                </div>
                <div>
                  <div>
                    <button
                      className="btn-primary"
                      onClick={handleWaveHello}
                      disabled={!connect}
                    >
                      caider waved
                    </button>
                    <button
                      className="btn-secondary"
                      onClick={stopWaveHello}
                      disabled={!connect}
                    >
                      stop waving
                    </button>
                  </div>
                </div>
              </div>
            </div>

            <aside className="col-md-3 display-right">
              <div className="text-center p-2">device caider</div>
              <ul className="list-box">
                <li>expansive: {expansive}</li>
                <li>micro controller: {micro}</li>
                <li>servo: {servo}</li>
                <li>step motor: {motor}</li>
                <li>Mode: hacking wifi</li>
              </ul>
              <hr />
              <div>
                <div
                  className="hacker-console"
                  style={{
                    background: "#080808ff",
                    color: "#0f0",
                    fontFamily: "monospace",
                    fontSize: 12,
                    padding: 12,
                    borderRadius: 6,
                    height: 280,
                    overflow: "hidden",
                    boxShadow: "inset 0 0 12px rgba(0,255,0,0.08)",
                    border: "1px solid white",
                  }}
                >
                  <div
                    className="hc-lines"
                    style={{
                      display: "inline-block",
                      animation: "hc-scroll 8s linear infinite",
                      whiteSpace: "pre",
                    }}
                  >
                    <div>boot sequence ... ok</div>
                    <div>link established: 192.168.0.42</div>
                    <div>handshake: AES-256 ... SUCCESS</div>
                    <div>spawn caider-agent</div>
                    <div>sensors: OK | motors: OK | mqtt: CONNECTED</div>
                    <div>uploading payload.bin ... 100%</div>
                    <div>exec: ./agent --stealth</div>
                    <div>clearing traces ... done</div>
                    <div>monitoring loop ...</div>
                    <div>waiting for commands</div>
                    <div>waiting for commands</div>
                    <div>waiting for commands</div>
                    <div>waiting for commands</div>
                    <div>waiting for commands</div>
                    <div>waiting for commands</div>
                    <div>waiting for commands</div>
                    <div>waiting for commands</div>
                    <div>101010110101010110101</div>
                    <div>1010101101010101101</div>
                    <div>10101011010101011</div>
                    <div>10101011010101</div>
                    <div>10101011010</div>
                    <div>10101010</div>
                    <div>10101</div>
                    <div>1</div>
                    <div>0</div>
                    <div>...</div>
                  </div>
                  <style>{`
                    @keyframes hc-scroll {
                      0% { transform: translateY(100%); }
                      100% { transform: translateY(-100%); }
                    }
                    .hacker-console .hc-lines > div { line-height: 1.4; padding: 1px 0; }
                    .hacker-console .hc-lines > div::before { content: "> "; color: #7cff7c; }
                  `}</style>
                </div>
              </div>
            </aside>
          </main>
        </div>
      </div>
    </>
  );
}
