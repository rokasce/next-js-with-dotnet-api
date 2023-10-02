import { useEffect, useState } from "react";
import useRefreshToken from "../hooks/useRefreshToken";
import useAuth from "../hooks/useAuth";

type Props = {
  children: React.ReactNode;
};

function PersistLogin({ children }: Props) {
  const [isLoading, setIsLoading] = useState(true);

  const { refresh } = useRefreshToken();
  const { auth } = useAuth();

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

  return <> {isLoading ? <div>Loading...</div> : children} </>;
}

export default PersistLogin;
