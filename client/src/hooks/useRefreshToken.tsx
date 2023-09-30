import { useRouter } from "next/navigation";
import { PUBLIC_API } from "@/lib/api";
import useAuthContext from "./useAuth";

export default function useRefreshToken() {
  const { setAuth } = useAuthContext();
  const { replace } = useRouter();

  const refresh = async () => {
    try {
      const response = await PUBLIC_API.post("/auth/refresh-token", null, {
        withCredentials: true,
      });

      if (response.status !== 200) {
        replace("/login");
      }

      setAuth((prev) => {
        return {
          ...prev,
          ...response.data,
        };
      });

      return response.data.accessToken;
    } catch (error) {
      // TODO: Handle error
      replace("/login");
    }
  };

  return { refresh };
}
