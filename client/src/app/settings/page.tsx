import { ProfileForm } from "@/components/auth/profile/profileForm";
import { Separator } from "@/components/ui/separator";

export default function ProfileSettingsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-lg font-medium">Profile</h3>
        <p className="text-sm text-muted-foreground">
          Update your account settings. Set your preferred language and
          timezone.
        </p>
      </div>
      <Separator />
      <ProfileForm />
    </div>
  );
}
