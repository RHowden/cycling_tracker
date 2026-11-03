import { useSubmitSession } from "../../hooks/useSubmitSession";
import { SportTypeEnum } from "../../api/schemas";
import { useNavigate } from "react-router";
import "./AdHocSession.css";

export function AdHocSession() {
    const { values, handleChange, submit, saving, saveError, fieldErrors } = useSubmitSession();
      const navigate = useNavigate();

    const handleFormSubmit = async (e: React.SubmitEvent<HTMLFormElement>) => {
        // 1. Trigger the hook's submission logic
        const success = await submit(e);

        // 2. If it succeeded, redirect the user
        if (success) {
            navigate("/dashboard"); // Adjust path to your activity history/dashboard page
        }
    };

    return (
        <div className="card card--form-layout card--narrow">
            <h2 className="card__title">Log New Activity Session</h2>

            {/* Global Backend Save Errors */}
            {saveError && (
                <div className="alert alert--error">
                    <div className="alert__title">Error Saving Session</div>
                    <ul className="alert__list">
                        {saveError.map((msg, idx) => (
                            <li key={idx} className="error-message">{msg}</li>
                        ))}
                    </ul>
                </div>
            )}

            <form onSubmit={handleFormSubmit} noValidate>
                {/* Sport Select */}
                <div className="form-field">
                    <label htmlFor="sport">Sport</label>
                    <select
                        id="sport"
                        name="sport"
                        value={values.sport}
                        onChange={handleChange}
                    >
                        <option value="">-- Select Sport --</option>
                        {SportTypeEnum.map((sport) => (
                            <option key={sport} value={sport}>
                                {sport}
                            </option>
                        ))}
                    </select>
                    {fieldErrors.sport && <p className="form-error">{fieldErrors.sport}</p>}
                </div>

                {/* Start Time */}
                <div className="form-field">
                    <label htmlFor="startTime">Start Time</label>
                    <input
                        id="startTime"
                        type="datetime-local"
                        name="startTime"
                        value={values.startTime}
                        onChange={handleChange}
                    />
                    {fieldErrors.startTime && <p className="form-error">{fieldErrors.startTime}</p>}
                </div>

                {/* Duration */}
                <div className="form-field">
                    <label htmlFor="durationSeconds">Duration (Seconds)</label>
                    <input
                        id="durationSeconds"
                        type="number"
                        name="durationSeconds"
                        value={values.durationSeconds}
                        onChange={handleChange}
                    />
                    {fieldErrors.durationSeconds && <p className="form-error">{fieldErrors.durationSeconds}</p>}
                </div>

                {/* Optional Metrics Row */}
                <div className="form-row-grid">
                    <div className="form-field">
                        <label htmlFor="avgHeartRate">Avg HR (bpm)</label>
                        <input
                            id="avgHeartRate"
                            type="number"
                            name="avgHeartRate"
                            value={values.avgHeartRate}
                            onChange={handleChange}
                            placeholder="Optional"
                        />
                        {fieldErrors.avgHeartRate && <p className="form-error">{fieldErrors.avgHeartRate}</p>}
                    </div>

                    <div className="form-field">
                        <label htmlFor="avgPowerWatts">Avg Power (Watts)</label>
                        <input
                            id="avgPowerWatts"
                            type="number"
                            name="avgPowerWatts"
                            value={values.avgPowerWatts}
                            onChange={handleChange}
                            placeholder="Optional"
                        />
                        {fieldErrors.avgPowerWatts && <p className="form-error">{fieldErrors.avgPowerWatts}</p>}
                    </div>
                </div>

                {/* Distance */}
                <div className="form-field">
                    <label htmlFor="distanceMetres">Distance (Meters)</label>
                    <input
                        id="distanceMetres"
                        type="number"
                        name="distanceMetres"
                        value={values.distanceMetres}
                        onChange={handleChange}
                        placeholder="Optional"
                    />
                    {fieldErrors.distanceMetres && <p className="form-error">{fieldErrors.distanceMetres}</p>}
                </div>

                {/* Submit */}
                <button type="submit" className="btn btn-primary" disabled={saving}>
                    {saving ? "Saving Activity..." : "Log Session"}
                </button>
            </form>
        </div>
    );
}
