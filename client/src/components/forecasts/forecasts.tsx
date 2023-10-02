"use client";

import { useEffect, useState } from "react";
import useAuthContext from "@/hooks/useAuth";
import useApi from "@/hooks/useApi";
import { Button } from "../ui/button";
import PersistLogin from "../persistLogin";

function Forecasts() {
  const { auth } = useAuthContext();

  const API = useApi();

  const [forecasts, setForecasts] = useState<Forecast[]>([]);

  async function getForecasts() {
    try {
      const result = await API.get("/weatherforecast", {
        withCredentials: true,
      });

      if (result.status === 200) {
        setForecasts(result.data);
      }
    } catch (error) {
      console.log(error);
    }
  }

  useEffect(() => {
    getForecasts();
  }, []);

  return (
    <PersistLogin>
      <section>
        <h1>Forecasts</h1>

        <p> {auth?.user?.email} </p>

        <ul>
          {forecasts.map((forecast) => (
            <li key={forecast.date.toString()}>
              {forecast.date.toString()}: {forecast.temperatureC}
            </li>
          ))}
        </ul>

        <Button onClick={() => getForecasts()}>Refresh</Button>
      </section>
    </PersistLogin>
  );
}

export default Forecasts;

type Forecast = {
  date: Date;
  temperatureC: number;
  summary: string;
};
