import { useEffect } from "react";
import { API } from "@/lib/api";
import useAuthContext from "./useAuth";
import useRefreshToken from "./useRefreshToken";

export default function useApi() {
  const { auth } = useAuthContext();
  const { refresh } = useRefreshToken();

  useEffect(() => {
    const requestInterceptor = API.interceptors.request.use(
      (config) => {
        if (!config.headers.Authorization) {
          config.headers.Authorization = `Bearer ${auth?.accessToken}`;
        }

        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    const responseInterceptor = API.interceptors.response.use(
      (response) => response,
      async (error) => {
        const previousRequest = error?.config;
        if (
          (error?.response?.status === 401 ||
            error?.response?.status === 403) &&
          !previousRequest?.sent
        ) {
          previousRequest.sent = true;
          const newAccessToken = await refresh();
          previousRequest.headers.Authorization = `Bearer ${newAccessToken}`;

          return API(previousRequest);
        }

        return Promise.reject(error);
      }
    );

    return () => {
      API.interceptors.request.eject(requestInterceptor);
      API.interceptors.response.eject(responseInterceptor);
    };
  }, [auth, refresh]);

  return { api: API };
}
