"use client";

import { createContext, useContext, useEffect, useState } from "react";
import useRefreshToken from "../hooks/useRefreshToken";
import useAuth from "../hooks/useAuth";

type UserContext = {
  isLoading: boolean;
  setLoading: React.Dispatch<React.SetStateAction<boolean>>;
};

type PersistLoginContextProviderProps = {
  children: React.ReactNode;
};

const PersistLoginContext = createContext<UserContext | null>(null);

export const PersistLoginProvider = ({
  children,
}: PersistLoginContextProviderProps) => {
  const { refresh } = useRefreshToken();
  const { auth } = useAuth();

  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const verifyRefreshToken = async () => {
      try {
        await refresh();
      } catch (error) {
        console.error(error);
      } finally {
        setIsLoading(false);
      }
    };

    if (!auth?.accessToken) {
      verifyRefreshToken();
    } else {
      setIsLoading(false);
    }
  }, []);

  return (
    <PersistLoginContext.Provider
      value={{
        isLoading,
        setLoading: setIsLoading,
      }}
    >
      {children}
    </PersistLoginContext.Provider>
  );
};

export function usePersistLoginContext() {
  const context = useContext(PersistLoginContext);

  if (!context) {
    throw new Error("usePersistLoginContext must be used within AuthProvider");
  }

  return context;
}
