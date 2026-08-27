export interface ZoneInput {
  name: string;
  price: number;
  capacity: number;
}

export interface CreateEventRequest {
  name: string;
  date: string;
  location: string;
  zones: ZoneInput[];
}
