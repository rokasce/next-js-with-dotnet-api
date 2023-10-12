"use client";

import { AbsoluteSpinner } from "@/components/ui/spinner";
import { usePersistLoginContext } from "@/context/persistLoginContext";

function Home() {
  const { isLoading } = usePersistLoginContext();

  if (isLoading) return <AbsoluteSpinner />;

  return <h1 className="text-4xl">Authenticated User homepage</h1>;
}

export default Home;
