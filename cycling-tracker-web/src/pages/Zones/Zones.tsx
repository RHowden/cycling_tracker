import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router';

import { useZones } from '../../hooks/useZones';
import { athleteZonesSchema, type AthleteZones, type ZoneSet } from '../../api/schemas';

import './Zones.css';

type ZoneKey = 'heartRate' | 'power';
type Field = keyof ZoneSet;

// Inputs hold strings while typing; we convert to numbers only to validate/save.
type DraftSet = Record<Field, string>;
type Draft = Record<ZoneKey, DraftSet>;
type FieldErrors = Partial<Record<ZoneKey, Partial<Record<Field, string>>>>;

const toDraft = (zones: AthleteZones): Draft => ({
    heartRate: {
        zone1Max: String(zones.heartRate.zone1Max),
        zone2Max: String(zones.heartRate.zone2Max),
    },
    power: {
        zone1Max: String(zones.power.zone1Max),
        zone2Max: String(zones.power.zone2Max),
    },
});

const fromDraft = (draft: Draft) => ({
    heartRate: {
        zone1Max: Number(draft.heartRate.zone1Max),
        zone2Max: Number(draft.heartRate.zone2Max),
    },
    power: {
        zone1Max: Number(draft.power.zone1Max),
        zone2Max: Number(draft.power.zone2Max),
    },
});

function validate(draft: Draft): { value?: AthleteZones; errors: FieldErrors } {
    const result = athleteZonesSchema.safeParse(fromDraft(draft));
    if (result.success) return { value: result.data, errors: {} };

    const errors: FieldErrors = {};
    for (const issue of result.error.issues) {
        const [set, field] = issue.path as [ZoneKey, Field];
        if (!set || !field) continue;
        const setErrors = (errors[set] ??= {});
        setErrors[field] ??= issue.message; // first error per field wins
    }
    return { errors };
}

const hasChanged = (a: AthleteZones, b: AthleteZones) =>
    (['heartRate', 'power'] as const).some(
        (k) => a[k].zone1Max !== b[k].zone1Max || a[k].zone2Max !== b[k].zone2Max,
    );

// Parses a draft string for the live zone 3 / zone 2 lower-bound display.
const toInt = (s: string): number | null => {
    const n = Number(s);
    return s.trim() !== '' && Number.isInteger(n) ? n : null;
};

interface ZoneInputProps {
    id: string;
    label: string;
    value: string;
    error?: string;
    disabled: boolean;
    onChange: (value: string) => void;
}

function ZoneInput({ id, label, value, error, disabled, onChange }: ZoneInputProps) {
    return (
        <>
            <input
                id={id}
                className="zone-input"
                type="number"
                inputMode="numeric"
                min={1}
                step={1}
                aria-label={label}
                aria-invalid={error ? true : undefined}
                aria-describedby={error ? `${id}-error` : undefined}
                value={value}
                disabled={disabled}
                onChange={(e) => onChange(e.target.value)}
            />
            {error && (
                <p id={`${id}-error`} className="field-error">
                    {error}
                </p>
            )}
        </>
    );
}

interface ZoneSetCardProps {
    setKey: ZoneKey;
    title: string;
    unit: string;
    zones: ZoneSet;
    draft: DraftSet | null; // null = view mode
    errors?: Partial<Record<Field, string>>;
    disabled: boolean;
    onChange: (field: Field, value: string) => void;
}

function ZoneSetCard({ setKey, title, unit, zones, draft, errors, disabled, onChange }: ZoneSetCardProps) {
    const headingId = `${setKey}-zones-heading`;

    // View mode reads the saved values; edit mode reads the draft so zones 2 and 3 update live.
    const zone1 = draft ? toInt(draft.zone1Max) : zones.zone1Max;
    const zone2 = draft ? toInt(draft.zone2Max) : zones.zone2Max;

    return (
        <section className="zone-card" aria-labelledby={headingId}>
            <h2 id={headingId}>{title}</h2>
            <dl className="zone-list">
                <div className="zone-row">
                    <dt>Zone 1</dt>
                    <dd>
                        {draft ? (
                            <div className="zone-value">
                                <span className="zone-inline">
                                    Up to{' '}
                                    <ZoneInput
                                        id={`${setKey}-zone1Max`}
                                        label={`${title} zone 1 max`}
                                        value={draft.zone1Max}
                                        error={errors?.zone1Max}
                                        disabled={disabled}
                                        onChange={(v) => onChange('zone1Max', v)}
                                    />{' '}
                                    {unit}
                                </span>
                            </div>
                        ) : (
                            <>Up to {zone1} {unit}</>
                        )}
                    </dd>
                </div>

                <div className="zone-row">
                    <dt>Zone 2</dt>
                    <dd>
                        {draft ? (
                            <div className="zone-value">
                                <span className="zone-inline">
                                    {zone1 === null ? '…' : zone1 + 1} to{' '}
                                    <ZoneInput
                                        id={`${setKey}-zone2Max`}
                                        label={`${title} zone 2 max`}
                                        value={draft.zone2Max}
                                        error={errors?.zone2Max}
                                        disabled={disabled}
                                        onChange={(v) => onChange('zone2Max', v)}
                                    />{' '}
                                    {unit}
                                </span>
                            </div>
                        ) : (
                            <>{zones.zone1Max + 1} to {zones.zone2Max} {unit}</>
                        )}
                    </dd>
                </div>

                <div className="zone-row">
                    <dt>Zone 3</dt>
                    <dd>Above {zone2 ?? '…'} {unit}</dd>
                </div>
            </dl>
        </section>
    );
}

export function Zones() {
    const navigate = useNavigate();
    const { data, loading, error, saving, saveError, save, clearSaveError } = useZones();

    // Hooks must run before the early returns below.
    // draft === null means view mode; otherwise we're editing.
    const [draft, setDraft] = useState<Draft | null>(null);
    const [justSaved, setJustSaved] = useState(false);

    if (loading) {
        return (
            <div className="session-loading" role="status" aria-live="polite">
                <p>Loading zones…</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="session-error-container" role="alert">
                <p className="error-title">Error loading zones:</p>
                <ul className="error-list">
                    {error.map((msg, i) => (
                        <li key={i}>{msg}</li>
                    ))}
                </ul>
                <button className="btn-secondary" onClick={() => navigate('/dashboard')}>
                    Return to Dashboard
                </button>
            </div>
        );
    }

    if (!data) {
        return (
            <div className="session-error-container">
                <p>No zones found.</p>
                <button className="btn-secondary" onClick={() => navigate('/dashboard')}>
                    Return to Dashboard
                </button>
            </div>
        );
    }

    const editing = draft !== null;
    const { value, errors } = draft ? validate(draft) : { value: undefined, errors: {} as FieldErrors };
    const canSave = !!value && hasChanged(value, data) && !saving;

    function startEdit() {
        setJustSaved(false);
        setDraft(toDraft(data!));
    }

    function cancelEdit() {
        clearSaveError();
        setDraft(null);
    }

    function updateField(set: ZoneKey, field: Field, text: string) {
        setDraft((d) => d && { ...d, [set]: { ...d[set], [field]: text } });
    }

    async function handleSubmit(e: FormEvent) {
        e.preventDefault();
        if (!value || !canSave) return;
        if (await save(value)) {
            setDraft(null);
            setJustSaved(true);
        }
    }

    return (
        <form className="zones-detail-container" onSubmit={handleSubmit} noValidate>
            <header className="zones-header">
                <h1>Training Zones</h1>

                {editing ? (
                    <div className="zones-actions">
                        <button key="cancel" type="button" className="btn-secondary" onClick={cancelEdit} disabled={saving}>
                            Cancel
                        </button>
                        <button key="save" type="submit" className="btn-primary" disabled={!canSave}>
                            {saving ? 'Saving…' : 'Save'}
                        </button>
                    </div>
                ) : (
                    <button key="edit" type="button" className="btn-primary" onClick={startEdit}>
                        Edit
                    </button>
                )}
            </header>

            {justSaved && !editing && (
                <p className="zones-saved" role="status">
                    Zones saved.
                </p>
            )}

            {saveError && (
                <div className="zones-banner" role="alert">
                    <p className="error-title">Couldn't save zones:</p>
                    <ul className="error-list">
                        {saveError.map((msg, i) => (
                            <li key={i}>{msg}</li>
                        ))}
                    </ul>
                </div>
            )}

            <div className="zone-cards">
                <ZoneSetCard
                    setKey="heartRate"
                    title="Heart rate"
                    unit="bpm"
                    zones={data.heartRate}
                    draft={draft?.heartRate ?? null}
                    errors={errors.heartRate}
                    disabled={saving}
                    onChange={(field, v) => updateField('heartRate', field, v)}
                />
                <ZoneSetCard
                    setKey="power"
                    title="Power"
                    unit="W"
                    zones={data.power}
                    draft={draft?.power ?? null}
                    errors={errors.power}
                    disabled={saving}
                    onChange={(field, v) => updateField('power', field, v)}
                />
            </div>
        </form>
    );
}