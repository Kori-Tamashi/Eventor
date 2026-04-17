import type { Web_Dtos_Gender } from '../generated';

export type LoginPayload = {
  phone: string;
  password: string;
};

export type RegisterPayload = {
  name: string;
  phone: string;
  gender: Web_Dtos_Gender;
  password: string;
};

export type AuthTokenResponse = {
  token: string;
};
