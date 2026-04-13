import { DayApiModel } from './day.models';

export type RegistrationApiModel = {
  id: string;
  eventId: string;
  userId: string;
  type: 0 | 1 | 2;
  payment: boolean;
  days: DayApiModel[];
};
