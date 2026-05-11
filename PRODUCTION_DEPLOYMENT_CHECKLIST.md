# Production Deployment Checklist

**Date**: 2026-01-07
**Changes**: Fixed Docker configuration for local setup
**Commits**: `87c3441` - Fixed Docker configuration for local setup

## Pre-Deployment Verification

### 1. Understanding the Changes ✅

**What Changed:**
- `docker-compose.yml`: `VITE_API_URL` changed from `http://localhost:5000` → `http://backend:8080`
- `frontend/src/services/api.ts`: Added conditional logic to use `/api` for Docker, full URL for production

**Why Production is Safe:**
- Production builds use `fly.toml` build args, NOT `docker-compose.yml`
- Production uses: `VITE_API_URL=https://vehicle-expense-backend.fly.dev/api`
- The new conditional logic only triggers for `http://backend` (local Docker), not production URLs
- Frontend is a static build - API URL is baked in at build time

### 2. Run Verification Script

```bash
cd "f:/Portfolio Projects/vehicle-expense-tracker"
bash verify-production-build.sh
```

**Expected Results:**
- ✅ Build completes successfully
- ✅ Production URL (`vehicle-expense-backend.fly.dev`) found in build
- ✅ NO Docker backend URL (`http://backend`) found in build

### 3. Manual Code Review

**Check `frontend/src/services/api.ts`:**
```typescript
// Should have this logic:
const API_BASE_URL = import.meta.env.VITE_API_URL?.startsWith('http://backend')
  ? '/api'  // Only for local Docker
  : import.meta.env.VITE_API_URL || '/api';  // Production
```

**Check `frontend/fly.toml`:**
```toml
[build.args]
  VITE_API_URL = "https://vehicle-expense-backend.fly.dev/api"
```

### 4. Backend Status Check

Before deploying frontend, ensure backend is healthy:

```bash
# Check if backend is running
curl https://vehicle-expense-backend.fly.dev/health

# Expected response: {"status":"Healthy"}
```

### 5. Test Local Docker Setup (Optional)

Verify the local Docker changes work as expected:

```bash
cd "f:/Portfolio Projects/vehicle-expense-tracker"

# Start all services
docker-compose up -d

# Test backend
curl http://127.0.0.1:5000/health

# Test frontend (should return 200)
curl -I http://127.0.0.1:5173

# Check frontend can reach backend through proxy
curl http://127.0.0.1:5173/api/health

# Stop services
docker-compose down
```

## Deployment Steps

### Option 1: Deploy Both (Recommended for First Time)

Even though backend hasn't changed, deploying both ensures they're in sync:

```bash
cd "f:/Portfolio Projects/vehicle-expense-tracker"

# 1. Deploy backend first (just to be safe)
cd backend
fly deploy

# Wait for deployment to complete and verify
curl https://vehicle-expense-backend.fly.dev/health

# 2. Deploy frontend
cd ../frontend
fly deploy

# Wait for deployment to complete
```

### Option 2: Deploy Frontend Only (Faster)

Since only frontend code changed:

```bash
cd "f:/Portfolio Projects/vehicle-expense-tracker/frontend"
fly deploy
```

## Post-Deployment Verification

### 1. Check Deployment Status

```bash
# Check frontend status
fly status -a vehicle-expense-frontend

# Check backend status
fly status -a vehicle-expense-backend

# View frontend logs
fly logs -a vehicle-expense-frontend

# View backend logs
fly logs -a vehicle-expense-backend
```

### 2. Test Production Application

Open browser and test:

1. **Frontend loads**: https://vehicle-expense-frontend.fly.dev
2. **Login works**: Use `demo@vehicletracker.com` / `Test123`
3. **API calls work**: Check Network tab in browser DevTools
   - Should see calls to `https://vehicle-expense-backend.fly.dev/api/*`
   - Should NOT see any calls to `http://backend:8080`
4. **Features work**:
   - View vehicles
   - Add expense
   - View dashboard

### 3. Monitor for Errors

```bash
# Watch frontend logs for errors
fly logs -a vehicle-expense-frontend

# Watch backend logs for errors
fly logs -a vehicle-expense-backend
```

## Rollback Plan (If Needed)

If something goes wrong:

```bash
# View deployment history
fly releases -a vehicle-expense-frontend

# Rollback to previous version (replace v2 with actual version)
fly releases rollback v2 -a vehicle-expense-frontend
```

## Expected Behavior

### Production (Fly.io)
- ✅ Frontend uses: `https://vehicle-expense-backend.fly.dev/api`
- ✅ API calls go directly to backend (no proxy)
- ✅ Built as static files served by Nginx

### Local Docker
- ✅ Frontend uses: `/api` (relative path)
- ✅ Vite dev server proxies `/api` requests to `http://backend:8080`
- ✅ Containers communicate via Docker network

## Risk Assessment

**Risk Level**: 🟢 **LOW**

**Reasoning:**
1. Changes only affect local Docker environment configuration
2. Production uses separate build configuration in `fly.toml`
3. Code changes are defensive (conditional logic that won't trigger in production)
4. Backend is unchanged
5. Can easily rollback if needed

## Notes

- The verification script simulates the Fly.io build process locally
- Always test the production URL after deployment
- Keep demo credentials for quick testing: `demo@vehicletracker.com` / `Test123`
- Frontend is a static site, so worst case, can rebuild and redeploy in ~2 minutes

## Success Criteria

Deployment is successful when:
- [ ] Frontend loads at production URL
- [ ] Login works with demo credentials
- [ ] API calls go to `vehicle-expense-backend.fly.dev`
- [ ] No console errors in browser
- [ ] No Docker-specific URLs in browser Network tab
- [ ] All features (vehicles, expenses, dashboard) work correctly

---

**Prepared by**: Claude Code
**Reviewed by**: [Your Name]
**Deployment Date**: [To be filled]
**Deployment Result**: [To be filled]
