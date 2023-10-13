import FormFooter from "@/components/auth/formFooter";
import FormHeader from "@/components/auth/formHeader";
import RegisterForm from "@/components/auth/register/registerForm";

export default function Register() {
  return (
    <div className="lg:p-8">
      <div className="mx-auto flex w-full flex-col justify-center space-y-6 sm:w-[350px]">
        <FormHeader
          title="Create an account"
          description="Enter your email below to create your account"
        />
        <RegisterForm />
        <FormFooter />
      </div>
    </div>
  );
}
