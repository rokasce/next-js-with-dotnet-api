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
import { Axios, AxiosError } from "axios";

const RegisterFormSchema = z
  .object({
    email: z.string().email(),
    password: z.string().min(8).max(255),
    confirmPassword: z.string().min(8).max(255),
  })
  .refine((data) => data.password === data.confirmPassword, {
    path: ["confirmPassword"],
    message: "Passwords do not match",
  });

function RegisterForm() {
  const { push } = useRouter();

  const [error, setError] = useState<string | null>(null);

  const form = useForm<z.infer<typeof RegisterFormSchema>>({
    resolver: zodResolver(RegisterFormSchema),
    defaultValues: {
      email: "",
      password: "",
      confirmPassword: "",
    },
  });

  async function onSubmit(values: z.infer<typeof RegisterFormSchema>) {
    setError(null);

    const { email, password, confirmPassword } = values;
    if (!email || !password || !confirmPassword) return;

    try {
      await PUBLIC_API.post(
        "/auth/register",
        JSON.stringify({ email, password }),
        {
          headers: {
            "Content-Type": "application/json",
          },
        }
      );

      push("/login");
    } catch (error: any) {
      const { response } = error;
      if (response.status === 400) {
        const { details } = response?.data?.at(0);

        setError(details ?? "Something went wrong");
        return;
      }

      if (response.status !== 200) {
        setError("Something went wrong, please try again");
        return;
      }
    }
  }

  return (
    <Card className='w-[375px]'>
      <CardHeader>
        <CardTitle>Register</CardTitle>
        <CardDescription>Authentication demo</CardDescription>
      </CardHeader>
      <CardContent>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)}>
            {form.formState.isSubmitting && <AbsoluteSpinner />}
            {error && error !== "" && (
              <FormMessage className='pb-2'>{error}</FormMessage>
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
            <FormField
              control={form.control}
              name='confirmPassword'
              render={({ field }) => (
                <FormItem className='my-4'>
                  <FormLabel>Confirm password</FormLabel>
                  <FormControl>
                    <Input
                      placeholder='Confirm your password'
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

          <div
            className='mx-auto my-4 flex w-full items-center justify-evenly 
          before:mr-4 before:block before:h-px before:flex-grow before:bg-stone-400 
          after:ml-4 after:block after:h-px after:flex-grow after:bg-stone-400'
          >
            or
          </div>
          <p>
            Already have an account?
            <Link href='/login' className='text-blue-500 hover:underline'>
              Login
            </Link>
          </p>
        </Form>
      </CardContent>
    </Card>
  );
}

export default RegisterForm;
