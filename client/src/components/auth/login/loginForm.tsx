"use client";

import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "../../ui/form";
import { Input } from "../../ui/input";
import { Button } from "../../ui/button";
import { PUBLIC_API } from "@/lib/api";
import useAuthContext from "@/hooks/useAuth";
import { useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import { useEffect, useState } from "react";
import { AbsoluteSpinner } from "../../ui/spinner";
import { usePersistLoginContext } from "@/context/persistLoginContext";
import { EyeClosedIcon, EyeOpenIcon } from "@radix-ui/react-icons";
import { PASSWORD_REGEX } from "@/lib/constants";
import { toast } from "@/components/ui/use-toast";

const LoginFormSchema = z.object({
  email: z.string().email(),
  password: z.string().min(6).regex(PASSWORD_REGEX).max(255),
});

export default function LoginForm() {
  const { isLoading } = usePersistLoginContext();
  const { setAuth } = useAuthContext();
  const { push } = useRouter();
  const searchParams = useSearchParams();

  const [error, setError] = useState<string | null>(null);
  const [passwordVisible, setPasswordVisible] = useState(false);

  const form = useForm<z.infer<typeof LoginFormSchema>>({
    resolver: zodResolver(LoginFormSchema),
    mode: "onChange",
    defaultValues: {
      email: "",
      password: "",
    },
  });

  // TODO: This does not work
  useEffect(() => {
    if (searchParams.has("error")) {
      console.log("This runs:", searchParams.get("error"));
      toast({
        title: "Error",
        variant: "destructive",
        description: "There was an logging in with external provider.",
      });
    }
  }, [searchParams]);

  async function onSubmit(values: z.infer<typeof LoginFormSchema>) {
    setError(null);

    const { email, password } = values;
    if (!email || !password) return;

    try {
      const result = await PUBLIC_API.post(
        "/auth/login",
        JSON.stringify({ email, password }),
        {
          headers: {
            "Content-Type": "application/json",
          },
          withCredentials: true,
        },
      );

      setAuth(result.data);

      push("/home");
    } catch (error: any) {
      const { status } = error?.response;
      toast({
        title: "Error",
        variant: "destructive",
        description: "There was an error logging into your account.",
      });

      if (status === 400) {
        setError("Invalid credentials");

        return;
      }

      if (status === 500) {
        setError("Something went wrong");

        return;
      }
    }
  }

  if (isLoading) return <AbsoluteSpinner />;

  return (
    <Form {...form}>
      <form
        method="POST"
        action={`${process.env.NEXT_PUBLIC_API_URL}/auth/login/Google`}
      >
        <Button type="submit" className="mt-1 w-full">
          Continue with Google
        </Button>
      </form>
      <div
        className="mx-auto my-4 flex w-full items-center justify-evenly 
          before:mr-4 before:block before:h-px before:flex-grow before:bg-stone-400 
          after:ml-4 after:block after:h-px after:flex-grow after:bg-stone-400"
      >
        or
      </div>
      {form.formState.isSubmitting && <AbsoluteSpinner />}
      <form onSubmit={form.handleSubmit(onSubmit)}>
        {error && <FormMessage className="pb-2">Error: {error}</FormMessage>}
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Username</FormLabel>
              <FormControl>
                <Input placeholder="Enter your email address" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem className="my-4">
              <FormLabel>Password</FormLabel>
              <FormControl>
                <div className="relative">
                  <Button
                    variant="ghost"
                    size="icon"
                    type="button"
                    className="absolute right-2"
                    onClick={() => setPasswordVisible(!passwordVisible)}
                  >
                    {passwordVisible ? <EyeOpenIcon /> : <EyeClosedIcon />}
                  </Button>
                  <Input
                    placeholder="Enter your password"
                    type={passwordVisible ? "text" : "password"}
                    {...field}
                  />
                </div>
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button
          type="submit"
          className="w-full"
          disabled={!form.formState.isValid}
        >
          Sign In
        </Button>
      </form>

      <p>
        If you don&apos;t have an account, please{" "}
        <Link href="/register" className="text-blue-500 hover:underline">
          Register
        </Link>
      </p>
    </Form>
  );
}
