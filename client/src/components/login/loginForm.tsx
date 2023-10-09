"use client";

import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "../ui/card";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "../ui/form";
import { Input } from "../ui/input";
import { Button } from "../ui/button";
import { PUBLIC_API } from "@/lib/api";
import useAuthContext from "@/hooks/useAuth";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useState } from "react";
import { AbsoluteSpinner } from "../ui/spinner";
import { usePersistLoginContext } from "@/context/persistLoginContext";

const LoginFormSchema = z.object({
  email: z.string().email(),
  password: z.string().min(8).max(255),
});

function LoginForm() {
  const { isLoading } = usePersistLoginContext();
  const { setAuth } = useAuthContext();
  const { push } = useRouter();

  const [error, setError] = useState<string | null>(null);

  const form = useForm<z.infer<typeof LoginFormSchema>>({
    resolver: zodResolver(LoginFormSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

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
        }
      );

      setAuth(result.data);

      push("/home");
    } catch (error: any) {
      const { status } = error?.response;
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
    <Card className='w-[375px] relative'>
      <CardHeader>
        <CardTitle>Login</CardTitle>
        <CardDescription>Authentication demo</CardDescription>
      </CardHeader>
      <CardContent>
        <Form {...form}>
          {form.formState.isSubmitting && <AbsoluteSpinner />}
          <form onSubmit={form.handleSubmit(onSubmit)}>
            {error && (
              <FormMessage className='pb-2'>Error: {error}</FormMessage>
            )}
            <FormField
              control={form.control}
              name='email'
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Username</FormLabel>
                  <FormControl>
                    <Input placeholder='Enter your email address' {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name='password'
              render={({ field }) => (
                <FormItem className='my-4'>
                  <FormLabel>Password</FormLabel>
                  <FormControl>
                    <Input
                      placeholder='Enter your password'
                      type='password'
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <Button
              type='submit'
              className='w-full'
              disabled={!form.formState.isValid}
            >
              Submit
            </Button>
          </form>
          <form
            method='POST'
            action={`${process.env.NEXT_PUBLIC_API_URL}/auth/login/Google`}
          >
            <Button type='submit' className='w-full mt-4'>
              Sign in with Google
            </Button>
          </form>
          <div
            className='mx-auto my-4 flex w-full items-center justify-evenly 
          before:mr-4 before:block before:h-px before:flex-grow before:bg-stone-400 
          after:ml-4 after:block after:h-px after:flex-grow after:bg-stone-400'
          >
            or
          </div>
          <p>
            If you don&apos;t have an account, please{" "}
            <Link href='/register' className='text-blue-500 hover:underline'>
              Register
            </Link>
          </p>
        </Form>
      </CardContent>
    </Card>
  );
}

export default LoginForm;
