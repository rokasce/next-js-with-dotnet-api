import "./globals.css";
import { AuthProvider } from "@/context/authContext";
import type { Metadata } from "next";
import { Inter } from "next/font/google";
import Navbar from "@/components/navbar";
import { PersistLoginProvider } from "@/context/persistLoginContext";
import { ThemeProvider } from "@/context/themeProvider";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "Create Next App",
  description: "Generated by create next app",
};

type Props = {
  children: React.ReactNode;
};

export default function RootLayout({ children }: Props) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={inter.className}>
        <ThemeProvider
          attribute="class"
          defaultTheme="system"
          enableSystem
          disableTransitionOnChange
        >
          <main className="flex h-screen flex-col items-center pt-[56px]">
            <AuthProvider>
              <PersistLoginProvider>
                <Navbar />
                {children}
              </PersistLoginProvider>
            </AuthProvider>
          </main>
        </ThemeProvider>
      </body>
    </html>
  );
}
