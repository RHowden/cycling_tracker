// src/components/ErrorElement/ErrorElement.tsx
import { useRouteError, useNavigate } from "react-router";
import "./ErrorElement.css";

export function ErrorElement() {
  const error: any = useRouteError();
  const navigate = useNavigate();

  // Log the full error to the browser console securely in all environments
  console.error("Caught routing error:", error);

  // Check if we are running in production mode
  const isProduction = import.meta.env.PROD;

  return (
    <div className="error-boundary-container">
      <div className="error-boundary-card">
        <span className="error-icon">🛠️</span>
        <h1>System Derailment</h1>
        <p>Something went wrong while loading this page template.</p>
        
        {/* ─── CONDITIONAL PROD HIDING ─── */}
        {isProduction ? (
          <p className="prod-error-msg">
            An unexpected error occurred. Our engineering team has been notified.
          </p>
        ) : (
          /* This section only renders on localhost / development mode */
          <pre className="error-stack-trace">
            {error?.statusText || error?.message || "Unknown Application Error"}
          </pre>
        )}

        <div className="error-actions">
          <button className="btn-secondary" onClick={() => navigate(-1)}>
            ← Go Back
          </button>
          <button className="btn-primary" onClick={() => window.location.reload()}>
            Retry Page
          </button>
        </div>
      </div>
    </div>
  );
}
