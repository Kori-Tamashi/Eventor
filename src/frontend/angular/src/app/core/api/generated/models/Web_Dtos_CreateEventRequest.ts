/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { System_DateOnly } from './System_DateOnly';
export type Web_Dtos_CreateEventRequest = {
    title: string;
    description?: string | null;
    startDate: System_DateOnly;
    locationId: string;
    daysCount: number;
    percent: number;
    createdByUserId: string;
};

