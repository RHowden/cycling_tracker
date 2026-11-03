import { useSubmitSession } from "../../hooks/useSubmitSession";
import { SportTypeEnum } from "../../api/schemas";
import { useNavigate } from "react-router";

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
        <div className="form-container">
            <h2>Log New Activity Session</h2>

            {/* Global Backend Save Errors */}
            {saveError && (
                <div className="error-summary">
                    {saveError.map((msg, idx) => (
                        <div key={idx} className="error-message">{msg}</div>
                    ))}
                </div>
            )}

            <form onSubmit={handleFormSubmit} noValidate>
                {/* Sport Select */}
                <div className="form-group">
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
                    {fieldErrors.sport && <p className="field-error">{fieldErrors.sport}</p>}
                </div>

                {/* Start Time */}
                <div className="form-group">
                    <label htmlFor="startTime">Start Time</label>
                    <input
                        id="startTime"
                        type="datetime-local"
                        name="startTime"
                        value={values.startTime}
                        onChange={handleChange}
                    />
                    {fieldErrors.startTime && <p className="field-error">{fieldErrors.startTime}</p>}
                </div>

                {/* Duration */}
                <div className="form-group">
                    <label htmlFor="durationSeconds">Duration (Seconds)</label>
                    <input
                        id="durationSeconds"
                        type="number"
                        name="durationSeconds"
                        value={values.durationSeconds}
                        onChange={handleChange}
                    />
                    {fieldErrors.durationSeconds && <p className="field-error">{fieldErrors.durationSeconds}</p>}
                </div>

                {/* Optional Metrics Row */}
                <div className="form-row-grid">
                    <div className="form-group">
                        <label htmlFor="avgHeartRate">Avg HR (bpm)</label>
                        <input
                            id="avgHeartRate"
                            type="number"
                            name="avgHeartRate"
                            value={values.avgHeartRate}
                            onChange={handleChange}
                            placeholder="Optional"
                        />
                        {fieldErrors.avgHeartRate && <p className="field-error">{fieldErrors.avgHeartRate}</p>}
                    </div>

                    <div className="form-group">
                        <label htmlFor="avgPowerWatts">Avg Power (Watts)</label>
                        <input
                            id="avgPowerWatts"
                            type="number"
                            name="avgPowerWatts"
                            value={values.avgPowerWatts}
                            onChange={handleChange}
                            placeholder="Optional"
                        />
                        {fieldErrors.avgPowerWatts && <p className="field-error">{fieldErrors.avgPowerWatts}</p>}
                    </div>
                </div>

                {/* Distance */}
                <div className="form-group">
                    <label htmlFor="distanceMetres">Distance (Meters)</label>
                    <input
                        id="distanceMetres"
                        type="number"
                        name="distanceMetres"
                        value={values.distanceMetres}
                        onChange={handleChange}
                        placeholder="Optional"
                    />
                    {fieldErrors.distanceMetres && <p className="field-error">{fieldErrors.distanceMetres}</p>}
                </div>

                {/* Submit */}
                <button type="submit" disabled={saving}>
                    {saving ? "Saving Activity..." : "Log Session"}
                </button>
            </form>
        </div>
    );
}
