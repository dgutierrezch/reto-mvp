import { useState } from "react";
import type { ZoneInput } from "../types/event";
import { createEvent } from "../api/eventsApi";

interface FormErrors {
  name?: string;
  date?: string;
  location?: string;
  zones?: string;
}

const emptyZone = (): ZoneInput => ({ name: "", price: 0, capacity: 1 });

export default function EventForm() {
  const [name, setName] = useState("");
  const [date, setDate] = useState("");
  const [location, setLocation] = useState("");
  const [zones, setZones] = useState<ZoneInput[]>([emptyZone()]);
  const [errors, setErrors] = useState<FormErrors>({});
  const [loading, setLoading] = useState(false);
  const [successId, setSuccessId] = useState<string | null>(null);
  const [apiError, setApiError] = useState<string | null>(null);

  const updateZone = (index: number, patch: Partial<ZoneInput>) => {
    setZones((prev) => prev.map((z, i) => (i === index ? { ...z, ...patch } : z)));
  };

  const addZone = () => setZones((prev) => [...prev, emptyZone()]);

  const removeZone = (index: number) =>
    setZones((prev) => (prev.length > 1 ? prev.filter((_, i) => i !== index) : prev));

  const validate = (): boolean => {
    const next: FormErrors = {};

    if (!name.trim()) next.name = "El nombre del evento es obligatorio.";
    if (!date) next.date = "La fecha es obligatoria.";
    if (!location.trim()) next.location = "El lugar es obligatorio.";

    const invalidZone = zones.some(
      (z) => !z.name.trim() || z.price < 0 || z.capacity <= 0
    );
    if (zones.length === 0 || invalidZone) {
      next.zones = "Cada zona necesita nombre, precio >= 0 y capacidad > 0.";
    }

    setErrors(next);
    return Object.keys(next).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setApiError(null);
    setSuccessId(null);

    if (!validate()) return;

    setLoading(true);
    try {
      const result = await createEvent({
        name: name.trim(),
        date: new Date(date).toISOString(),
        location: location.trim(),
        zones
      });
      setSuccessId(result.id);
      setName("");
      setDate("");
      setLocation("");
      setZones([emptyZone()]);
      setErrors({});
    } catch (err) {
      setApiError(err instanceof Error ? err.message : "Error inesperado al guardar el evento.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-2xl mx-auto p-6">
      <h1 className="text-2xl font-semibold text-slate-800 mb-6">Registrar evento</h1>

      <form onSubmit={handleSubmit} className="space-y-5" noValidate>
        <div>
          <label className="block text-sm font-medium text-slate-700 mb-1">Nombre del evento</label>
          <input
            type="text"
            value={name}
            onChange={(e) => setName(e.target.value)}
            className="w-full rounded-md border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            placeholder="Ej. Festival de Rock 2026"
          />
          {errors.name && <p className="text-sm text-red-600 mt-1">{errors.name}</p>}
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Fecha</label>
            <input
              type="datetime-local"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              className="w-full rounded-md border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
            {errors.date && <p className="text-sm text-red-600 mt-1">{errors.date}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Lugar</label>
            <input
              type="text"
              value={location}
              onChange={(e) => setLocation(e.target.value)}
              className="w-full rounded-md border border-slate-300 px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              placeholder="Ej. Estadio Nacional"
            />
            {errors.location && <p className="text-sm text-red-600 mt-1">{errors.location}</p>}
          </div>
        </div>

        <div>
          <div className="flex items-center justify-between mb-2">
            <label className="block text-sm font-medium text-slate-700">Zonas</label>
            <button
              type="button"
              onClick={addZone}
              className="text-sm text-indigo-600 hover:text-indigo-800 font-medium"
            >
              + Agregar zona
            </button>
          </div>

          <div className="space-y-3">
            {zones.map((zone, index) => (
              <div key={index} className="grid grid-cols-12 gap-2 items-start bg-slate-50 p-3 rounded-md">
                <input
                  type="text"
                  value={zone.name}
                  onChange={(e) => updateZone(index, { name: e.target.value })}
                  placeholder="Nombre (ej. VIP)"
                  className="col-span-5 rounded-md border border-slate-300 px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
                <input
                  type="number"
                  min={0}
                  step="0.01"
                  value={zone.price}
                  onChange={(e) => updateZone(index, { price: Number(e.target.value) })}
                  placeholder="Precio"
                  className="col-span-3 rounded-md border border-slate-300 px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
                <input
                  type="number"
                  min={1}
                  value={zone.capacity}
                  onChange={(e) => updateZone(index, { capacity: Number(e.target.value) })}
                  placeholder="Capacidad"
                  className="col-span-3 rounded-md border border-slate-300 px-2 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
                <button
                  type="button"
                  onClick={() => removeZone(index)}
                  className="col-span-1 text-slate-400 hover:text-red-600 text-sm py-1.5"
                  aria-label="Eliminar zona"
                >
                  ✕
                </button>
              </div>
            ))}
          </div>
          {errors.zones && <p className="text-sm text-red-600 mt-1">{errors.zones}</p>}
        </div>

        {apiError && (
          <div className="rounded-md bg-red-50 border border-red-200 px-3 py-2 text-sm text-red-700">
            {apiError}
          </div>
        )}

        {successId && (
          <div className="rounded-md bg-green-50 border border-green-200 px-3 py-2 text-sm text-green-700">
            Evento creado correctamente. ID: {successId}
          </div>
        )}

        <button
          type="submit"
          disabled={loading}
          className="w-full sm:w-auto rounded-md bg-indigo-600 px-5 py-2.5 text-white font-medium hover:bg-indigo-700 disabled:opacity-60 disabled:cursor-not-allowed"
        >
          {loading ? "Guardando..." : "Guardar"}
        </button>
      </form>
    </div>
  );
}
