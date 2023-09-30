import axios from "axios";

const BASE_URL = "http://localhost:5102/api";

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
