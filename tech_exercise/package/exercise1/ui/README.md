# ACTS UI

A read-only Angular 21 + Material UI for the **Astronaut Career Tracking System** (ACTS). It consumes the StargateAPI's GET endpoints and presents the roster as an astronaut-focused dashboard.

## Quick reference

```bash
npm install         # first-time install
npm start           # ng serve on http://localhost:4200
npm test            # one-shot Vitest run
npm run build       # production build into dist/ui
```

The dev server expects the API to be running at `http://localhost:5204`. A Development-only CORS policy in the API allows the SPA to call it directly.

## Architecture at a glance

```
src/app/
  core/         models, tokens, interceptors, services
                (PeopleService, AstronautDutyService, NotificationService, LoadingService)
  shared/       reusable UI primitives (status chip, empty state) and pipes (date-only)
  features/
    home/       hero + at-a-glance stat cards
    people/     roster table + per-astronaut detail with duty timeline
```

API responses are surfaced via `ngx-toastr` through `NotificationService`:

- **4xx** -> warning toast carrying `BaseResponse.message`.
- **5xx** or network failure -> error toast with a generic message; the original error is logged to the console for diagnostics.

The UI does not call any add/ update endpoints because ACTS data is updated by an external service.
