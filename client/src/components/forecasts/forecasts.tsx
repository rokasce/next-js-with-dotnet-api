"use client";

import { useEffect, useState } from "react";
import useApi from "@/hooks/useApi";
import { usePersistLoginContext } from "../../context/persistLoginContext";
import { AbsoluteSpinner } from "../ui/spinner";

export default function Forecasts() {
  const { api } = useApi();
  const { isLoading } = usePersistLoginContext();

  const [forecasts, setForecasts] = useState<Forecast[]>([]);

  async function getForecasts() {
    try {
      const result = await api.get("/weatherforecast", {
        withCredentials: true,
      });

      setForecasts(result.data);
    } catch (error: any) {
      const { status } = error?.response;
      if (status === 500) {
        console.log("Something went wrong");
      }

      console.log(error);
    }
  }

  useEffect(() => {
    getForecasts();
  }, []);

  if (isLoading) return <AbsoluteSpinner />;

  return (
    <section>
      <ul>
        {forecasts.map((forecast) => (
          <li key={forecast.date.toString()}>
            {forecast.date.toString()}: {forecast.temperatureC} -{" "}
            {forecast.summary}
          </li>
        ))}
      </ul>
    </section>
  );
}

type Forecast = {
  date: Date;
  temperatureC: number;
  summary: string;
};
