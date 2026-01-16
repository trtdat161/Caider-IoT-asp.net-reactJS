import { useState } from "react";
import "./css/login.css";
import { useNavigate } from "react-router-dom";

export function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [errorUsername, setErrorUsername] = useState("");
  const [errorPassword, setErrorPassword] = useState("");
  const navigate = useNavigate();

  const LoginAction = (e) => {
    e.preventDefault();
    if (username === "") {
      setErrorUsername("empty username !");
    } else if (password === "") {
      setErrorPassword("empty password !");
    } else {
      navigate("/dashboard");
    }
  };

  return (
    <>
      <div className="container vh-100 d-flex justify-content-center align-items-center">
        {/* error */}

        <form onSubmit={LoginAction} className="border rounded shadow p-4">
          <h2 className="text-center text-light">LOGIN</h2>
          <hr />
          <div className="mb-3">
            <label htmlFor="username" className="form-label text-light">
              username
            </label>
            <br />
            <input
              type="text"
              id="username"
              placeholder="username"
              className="form-control"
              onChange={(e) => setUsername(e.target.value)}
            />
            {errorUsername && (
              <span className="error-message text-danger">{errorUsername}</span>
            )}
          </div>

          <div>
            <label htmlFor="password" className="form-label text-light">
              password
            </label>
            <br />
            <input
              type="password"
              id="password"
              placeholder="password"
              className="form-control"
              onChange={(e) => setPassword(e.target.value)}
            />
            {errorPassword && (
              <span className="error-message text-danger">{errorPassword}</span>
            )}
          </div>
          <div className="text-center">
            <button type="submit" className="w-100 mt-2">
              login
            </button>
          </div>
        </form>
      </div>
    </>
  );
}
