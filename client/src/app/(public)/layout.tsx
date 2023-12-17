import { NavigationBar } from '@/components/navbar';


export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <NavigationBar />
      {children}
    </>
  );
}
