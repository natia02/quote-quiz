import client from "../../shared/api/client";
import type { GameHistory, UserStats } from "./types";

export const getMyHistory = () =>
  client.get<GameHistory[]>("/gamehistory/my-history");

export const getMyStats = () =>
  client.get<UserStats>("/gamehistory/my-statistics");
