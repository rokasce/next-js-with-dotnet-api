import { usePathname, useRouter } from "next/navigation";

import { toast } from "@/components/ui/use-toast";
import { PUBLIC_API } from "@/lib/api";
import useAuthContext from "./useAuth";

export default function useRefreshToken() {
  const { setAuth } = useAuthContext();
  const { replace } = useRouter();
  const pathname = usePathname();

  const refresh = async () => {
    if (pathname === "/") return;

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
      toast({
        title: "Error",
        variant: "destructive",
        description: "There was an error refreshing your session.",
      });
      replace("/login");
    }
  };

  return { refresh };
}
