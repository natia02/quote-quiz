import client from '../../shared/api/client';
import type { UserDto, CreateUserRequest, QuoteDto, CreateQuoteRequest, AchievementItem } from './types';

// Users
export const getUsers = () => client.get<UserDto[]>('/user');
export const createUser = (data: CreateUserRequest) => client.post<UserDto>('/user', data);
export const updateUser = (id: number, data: UserDto) => client.put<UserDto>(`/user/${id}`, data);
export const disableUser = (id: number) => client.patch(`/user/${id}/disable`);
export const deleteUser = (id: number) => client.delete(`/user/${id}`);

// Quotes
export const getQuotes = () => client.get<QuoteDto[]>('/quotes');
export const createQuote = (data: CreateQuoteRequest) => client.post<QuoteDto>('/quotes', data);
export const updateQuote = (id: number, data: CreateQuoteRequest) => client.put<QuoteDto>(`/quotes/${id}`, data);
export const deleteQuote = (id: number) => client.delete(`/quotes/${id}`);

// Achievements
export const getAllHistory = () => client.get<AchievementItem[]>('/gamehistory');
