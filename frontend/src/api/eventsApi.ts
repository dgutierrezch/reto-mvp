import type { CreateEventRequest } from "../types/event";

const API_BASE_URL = import.meta.env.VITE_EVENT_API_URL ?? "http://localhost:5001";

// Token fijo para la demo, tal como permite el reto ("puede ser fijo para la demo").
// En un entorno real vendría del flujo de login/OIDC.
const DEMO_TOKEN = import.meta.env.VITE_DEMO_JWT ?? "";

export async function createEvent(payload: CreateEventRequest): Promise<{ id: string }> {
  const response = await fetch(`${API_BASE_URL}/events`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${DEMO_TOKEN}`
    },
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new Error(body?.title ?? `Error ${response.status} al crear el evento`);
  }

  return response.json();
}
