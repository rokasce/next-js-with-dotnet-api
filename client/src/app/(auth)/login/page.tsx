import FormHeader from "@/components/auth/formHeader";
import LoginForm from "@/components/auth/login/loginForm";
import FormFooter from "@/components/auth/formFooter";

export default function Login() {
  return (
    <div className="lg:p-8">
      <div className="mx-auto flex w-full flex-col justify-center space-y-6 sm:w-[350px]">
        <FormHeader
          title="Welcome back"
          description="Sign in to your account to continue"
        />
        <LoginForm />
        <FormFooter />
      </div>
    </div>
  );
}
