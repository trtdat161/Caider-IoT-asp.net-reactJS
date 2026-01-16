import { useEffect, useState } from "react";
import { LinkPage } from "./LinkPage";
import "../css/addOrUp.css";
import { useNavigate, useParams } from "react-router-dom";
import axios from "axios";

export function ExpansiveForm() {
  const [success, setSuccess] = useState("");
  const [errorEx, setErrorEx] = useState("");
  const [errorMic, setErrorMic] = useState("");
  const [name, setName] = useState("");
  const [note, setNote] = useState("");
  // FK micro
  const [microControlId, setMicroControlId] = useState("");
  const [microcontrollers, setMicrocontrollers] = useState([]);
  const navigation = useNavigate();
  const { id } = useParams();

  const isEditMode = Boolean(id);

  useEffect(() => {
    const fetchMicro = async () => {
      try {
        const response = await axios.get(`/api/microcontrollers`);
        console.log("data: " + response.data);
        const result = response.data;
        setMicrocontrollers(result.items || []);
      } catch (error) {
        console.error("Lỗi khi lấy dữ liệu:", error);
        alert("Lỗi khi lấy dữ liệu microcontroller: " + error.message);
      }
    };
    fetchMicro();
  }, []);

  useEffect(() => {
    if (isEditMode) {
      const fetchExpansive = async () => {
        try {
          const response = await axios.get(`/api/expansiveboards/${id}`);
          setName(response.data.name || "");
          setNote(response.data.note || "");
          setMicroControlId(response.data.microControlId || "");
        } catch (error) {
          console.error("Lỗi khi lấy dữ liệu:", error);
          alert("Lỗi khi lấy dữ liệu expansive board: " + error.message);
        }
      };
      fetchExpansive();
    }
  }, [id]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!name) {
      setErrorEx("tên board mở rộng là bắt buộc !");
      return;
    }
    if (!microControlId) {
      setErrorMic("Hãy chọn vi điều khiển tương ứng !");
      return;
    }
    try {
      // object data
      const data = {
        name,
        note,
        microControlId: parseInt(microControlId), // ép kiểu
      };
      if (isEditMode) {
        await axios.put(`/api/expansiveboards/${id}`, data); // ko dùng lại {} vì đó là object
        setSuccess("Expansive board updated successfully !");
      } else {
        const response = await axios.post(`/api/expansiveboards`, data); // ko dùng lại {} vì đó là object
        setErrorEx("");
        setErrorMic("");
        setMicroControlId("");
        setName("");
        setNote("");
        e.target.reset();
        setSuccess("Expansive board added successfully !");
        console.log("Đã gửi ok: " + response.data);
      }
    } catch (error) {
      console.error("Lỗi:", error);
      alert("Lỗi: " + error.message);
    }
  };

  useEffect(() => {
    const time = setTimeout(() => {
      setSuccess("");
    }, 2300);
    return () => clearTimeout(time);
  }, [success]);

  const title = isEditMode
    ? "Update Expansive Board"
    : "Add New Expansive Board";
  const buttonText = isEditMode
    ? "Update Expansive Board"
    : "Add Expansive Board";

  return (
    <>
      <div className="robot-container">
        <div className="d-flex justify-content-between align-items-center mb-4">
          <LinkPage />
          <button
            className="btn btn-secondary d-flex align-items-center gap-2"
            onClick={() => navigation("/manage/expansiveboard")}
          >
            <i className="bi bi-arrow-left"></i>
            Back
          </button>
        </div>
        <div className="robot-header">
          <div className="robot-icon">
            <i className="bi bi-robot"></i>
          </div>
          <h1 className="robot-title">
            <span className="glitch" data-text={title}>
              {title}
            </span>
          </h1>
        </div>
        <form action="" className="robot-form" onSubmit={handleSubmit}>
          <div className="name-micro input-group-robot">
            <label htmlFor="microcontroller" className="robot-label">
              <i className="bi bi-cpu"></i>
              Select Microcontroller
            </label>
            <div className="input-wrapper mb-4">
              <select
                id="microcontroller"
                className="form-select robot-input"
                value={microControlId}
                onChange={(e) => {
                  setMicroControlId(e.target.value);
                  setErrorMic("");
                }}
              >
                <option value="" className="text-center">
                  -- Select Microcontroller --
                </option>
                {microcontrollers.map((micro) => (
                  <option
                    key={micro.id}
                    value={micro.id}
                    className="text-center"
                  >
                    {micro.name}
                  </option>
                ))}
              </select>
              {errorMic && (
                <span className="error-message text-danger">{errorMic}</span>
              )}
            </div>

            <label htmlFor="name" className="robot-label">
              <i className="bi bi-cpu-fill"></i>
              Expansive Board Name
            </label>
            <div className="input-wrapper mb-4">
              <input
                type="text"
                id="name"
                name="name"
                className="form-control robot-input"
                placeholder="Enter microcontroller model..."
                value={name}
                onChange={(e) => {
                  setName(e.target.value);
                  setErrorEx("");
                }}
              />
              <span className="input-border"></span>
              {errorEx && (
                <span className="error-message text-danger">{errorEx}</span>
              )}
            </div>

            <label htmlFor="note" className="robot-label">
              <i className="bi bi-journal-code"></i>
              Technical Notes
            </label>
            <div className="input-wrapper">
              <textarea
                id="note"
                name="note"
                className="form-control robot-input"
                placeholder="Specifications, features, version..."
                rows="4"
                value={note}
                onChange={(e) => setNote(e.target.value)}
              ></textarea>
              <span className="input-border"></span>
            </div>
          </div>

          <button type="submit" className="robot-btn">
            <span className="btn-content">
              <i className="bi bi-lightning-charge-fill"></i>
              {buttonText}
            </span>
            <span className="btn-glow"></span>
          </button>

          {/* success */}
          {success && (
            <div className="alert alert-success text-center mt-3" role="alert">
              {success}
            </div>
          )}
          <div className="tech-lines"></div>
        </form>
      </div>
    </>
  );
}
