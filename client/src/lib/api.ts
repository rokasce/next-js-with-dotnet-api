import axios from "axios";

const BASE_URL = "https://localhost:7014";

export const PUBLIC_API = axios.create({
  baseURL: BASE_URL,
});

export const API = axios.create({
  baseURL: BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true,
});
