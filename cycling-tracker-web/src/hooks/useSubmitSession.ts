import { useState, ChangeEvent } from "react";
import { z } from "zod";
import { apiRequestVoid } from "../api/client";
import { CreateSessionSchema, SportTypeEnum } from "../api/schemas";
import { ApiError } from "../api/apiErrors";

interface FormState {
    sport: typeof SportTypeEnum[number] | "";
    startTime: string;
    durationSeconds: string;
    avgHeartRate: string;
    avgPowerWatts: string;
    distanceMetres: string;
}

const defaultValues: FormState = {
    sport: "",
    startTime: "",
    durationSeconds: "3600",
    avgHeartRate: "",
    avgPowerWatts: "",
    distanceMetres: "",
};

export function useSubmitSession() {
    const [values, setValues] = useState<FormState>(defaultValues);
    const [saving, setSaving] = useState(false);
    const [saveError, setSaveError] = useState<string[] | null>(null);
    const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

    const handleChange = (e: ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        setValues((prev) => ({
            ...prev,
            [name]: value,
        }));
    };

    const submit = async (e: React.SubmitEvent<HTMLFormElement>): Promise<boolean> => {
        e.preventDefault();
        setSaving(true);
        setSaveError(null);
        setFieldErrors({});

        const parsedPayload = {
            sport: values.sport,
            startTime: values.startTime ? new Date(values.startTime) : new Date(NaN),
            durationSeconds: values.durationSeconds === "" ? NaN : Number(values.durationSeconds),
            avgHeartRate: values.avgHeartRate === "" ? null : Number(values.avgHeartRate),
            avgPowerWatts: values.avgPowerWatts === "" ? null : Number(values.avgPowerWatts),
            distanceMetres: values.distanceMetres === "" ? null : Number(values.distanceMetres),
        };

        const validation = CreateSessionSchema.safeParse(parsedPayload);

        if (!validation.success) {
            const flattened = z.flattenError(validation.error);
            const errors: Record<string, string> = {};

            Object.entries(flattened.fieldErrors).forEach(([fieldKey, fieldMessages]) => {
                if (fieldMessages && fieldMessages.length > 0) {
                    errors[fieldKey] = fieldMessages[0];
                }
            });

            setFieldErrors(errors);
            setSaving(false);
            return false;
        }

        try {
            const validData = validation.data;

            // Map frontend validation objects to exact C# expected wire contracts
            const backendPayload = {
                ...validData,
                // Maps "Cycling" -> 2, matching C# internal backend enum integers
                sport: SportTypeEnum.indexOf(validData.sport as any),
                // Serializes JS Dates cleanly over HTTP wire pipelines
                startTime: validData.startTime.toISOString(),
            };

            await apiRequestVoid("/sessions", {
                method: "POST",
                body: backendPayload, 
            });

            setValues(defaultValues);
            return true;
        } catch (err) {
            // Restored back to your original direct assignment behavior
            setSaveError(err instanceof ApiError ? err.messages : ["Unknown backend processing error"]);
            return false;
        } finally {
            setSaving(false);
        }
    };

    function clearSaveError() {
        setSaveError(null);
    }

    return { values, handleChange, submit, saving, saveError, fieldErrors, clearSaveError };
}
