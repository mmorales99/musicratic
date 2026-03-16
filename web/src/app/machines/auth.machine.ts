/**
 * Re-export from the auth feature module.
 * The canonical auth machine lives in features/auth/machines/auth.machine.ts.
 */
export {
  authMachine,
  type AuthMachineContext,
  type AuthMachineEvent,
} from "@app/features/auth/machines/auth.machine";
