export type DayApiModel = {
  id: string;
  eventId: string;
  menuId: string;
  title: string;
  description?: string | null;
  sequenceNumber: number;
};
