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
} from "@/components/ui/form";
import { Button } from "@/components/ui/button";
import { EyeClosedIcon, EyeOpenIcon } from "@radix-ui/react-icons";
import { useState } from "react";
import { Input } from "@/components/ui/input";
import useApi from "@/hooks/useApi";
import { AxiosError } from "axios";
import { toast } from "@/components/ui/use-toast";

const ChangePasswordFormSchema = z
  .object({
    currentPassword: z.string().min(8).max(255),
    newPassword: z.string().min(8).max(255),
  })
  .refine((data) => data.newPassword !== data.currentPassword, {
    message: "New password must be different from current password",
    path: ["newPassword"],
  });

type ChangePasswordValues = z.infer<typeof ChangePasswordFormSchema>;

export default function ChangePasswordForm() {
  const [passwordVisible, setPasswordVisible] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { api } = useApi();

  const form = useForm<z.infer<typeof ChangePasswordFormSchema>>({
    resolver: zodResolver(ChangePasswordFormSchema),
    mode: "onBlur",
    defaultValues: {
      currentPassword: "",
      newPassword: "",
    },
  });

  async function onSubmit(values: ChangePasswordValues) {
    setError(null);

    const { currentPassword, newPassword } = values;

    if (!currentPassword || !newPassword) {
      setError("Please fill out all fields");
      return;
    }

    try {
      await api.post(
        "/auth/change-password",
        JSON.stringify({ currentPassword, newPassword }),
        {
          headers: {
            "Content-Type": "application/json",
          },
        },
      );

      form.reset();

      toast({
        title: "Success",
        description: <span>Password changed successfully!</span>,
      });
    } catch (error) {
      if (error instanceof AxiosError) {
        if (error.response?.status === 400) {
          setError("Make sure your current password is correct");
        }

        if (error.response?.status === 500) {
          setError(
            "Something went wrong, please contact support if this persists",
          );
        }
      } else {
        setError("Something went wrong");
      }
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)}>
        {error && error !== "" && (
          <FormMessage className="pb-2">{error}</FormMessage>
        )}
        <FormField
          control={form.control}
          name="currentPassword"
          render={({ field }) => (
            <FormItem className="my-4">
              <FormLabel>Current Password</FormLabel>
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

        <FormField
          control={form.control}
          name="newPassword"
          render={({ field }) => (
            <FormItem className="my-4">
              <FormLabel>New Password</FormLabel>
              <FormControl>
                <Input
                  placeholder="Enter your new password"
                  type={passwordVisible ? "text" : "password"}
                  {...field}
                />
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
          Change password
        </Button>
      </form>
    </Form>
  );
}
