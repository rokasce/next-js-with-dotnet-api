import { UserNavigationBar } from "@/components/navbar";

export default function Layout({ children }: { children: ReactNode }) {
  
  return (
    <>
        <UserNavigationBar />
        {children}
    </>
  );
}
