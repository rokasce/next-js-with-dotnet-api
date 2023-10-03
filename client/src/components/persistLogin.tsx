"use client";

import { createContext, useContext, useEffect, useState } from "react";
import useRefreshToken from "../hooks/useRefreshToken";
import useAuth from "../hooks/useAuth";

type UserContext = {
  isLoading: boolean;
  setLoading: React.Dispatch<React.SetStateAction<boolean>>;
};

type UserContextProviderProps = {
  children: React.ReactNode;
};

const UserContext = createContext<UserContext | null>(null);

export const UserContextProvider = ({ children }: UserContextProviderProps) => {
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
    <UserContext.Provider
      value={{
        isLoading,
        setLoading: setIsLoading,
      }}
    >
      {children}
    </UserContext.Provider>
  );
};

export function useUserContext() {
  const context = useContext(UserContext);

  if (!context) {
    throw new Error("useUserContext must be used within AuthProvider");
  }

  return context;
}
