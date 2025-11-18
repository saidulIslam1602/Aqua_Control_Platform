# Deprecated Files Cleanup Checklist

## ⚠️ Files to Remove After Testing Period

**Recommendation:** Keep these files for 1-2 sprints to ensure smooth transition, then delete them.

### 📋 Deprecated Components

#### Layout Components
- [ ] `/frontend/src/components/layout/AppLayout.vue`
  - **Replaced by:** `ModernLayout.vue`
  - **Last Used:** Before modernization
  - **Safe to delete:** After production testing

#### View Components
- [ ] `/frontend/src/views/TankList.vue`
  - **Replaced by:** `views/tanks/ModernTankListView.vue`
  - **Router updated:** ✅ Yes
  - **Safe to delete:** After verification

- [ ] `/frontend/src/views/TankDetail.vue`
  - **Replaced by:** `views/tanks/ModernTankDetailView.vue`
  - **Router updated:** ✅ Yes
  - **Safe to delete:** After verification

- [ ] `/frontend/src/views/sensor/SensorsView.vue` (old)
  - **Replaced by:** `views/sensors/ModernSensorsView.vue`
  - **Router updated:** ✅ Yes
  - **Safe to delete:** After verification

- [ ] `/frontend/src/views/analytics/AnalyticsView.vue` (old)
  - **Replaced by:** `views/analytics/ModernAnalyticsView.vue`
  - **Router updated:** ✅ Yes
  - **Safe to delete:** After verification

- [ ] `/frontend/src/views/settings/SettingsView.vue` (old)
  - **Replaced by:** `views/settings/ModernSettingsView.vue`
  - **Router updated:** ✅ Yes
  - **Safe to delete:** After verification

---

## 🔍 Verification Steps Before Deletion

### 1. **Test All Routes**
```bash
# Navigate to each route and verify modern views load
- /dashboard
- /tanks
- /tanks/:id
- /sensors
- /analytics
- /settings
```

### 2. **Check for Imports**
```bash
# Search for any remaining imports of old files
cd frontend/src
grep -r "AppLayout" . --exclude-dir=node_modules
grep -r "TankList\.vue" . --exclude-dir=node_modules
grep -r "TankDetail\.vue" . --exclude-dir=node_modules
grep -r "SensorsView\.vue" . --exclude-dir=node_modules
grep -r "AnalyticsView\.vue" . --exclude-dir=node_modules
grep -r "SettingsView\.vue" . --exclude-dir=node_modules
```

### 3. **Run Tests**
```bash
# Ensure all tests pass
npm run test:unit
npm run test:e2e
```

### 4. **Build Verification**
```bash
# Verify production build succeeds
npm run build
```

---

## 📅 Cleanup Timeline

| Phase | Duration | Action |
|-------|----------|--------|
| **Week 1-2** | Testing | Use modern views in development |
| **Week 3-4** | Staging | Deploy to staging environment |
| **Week 5-6** | Production | Monitor production usage |
| **Week 7** | Cleanup | Remove deprecated files if stable |

---

## 🗑️ Deletion Commands

**When ready to delete, run:**

```bash
cd /home/saidul/Desktop/Portfolio/AquaCulturePlatform/AquaControl-Platform/frontend/src

# Remove old layout
rm components/layout/AppLayout.vue

# Remove old views
rm views/TankList.vue
rm views/TankDetail.vue

# Remove old sensor view (if exists in old location)
rm views/sensor/SensorsView.vue 2>/dev/null || true

# Remove old analytics view
rm views/analytics/AnalyticsView.vue 2>/dev/null || true

# Remove old settings view
rm views/settings/SettingsView.vue 2>/dev/null || true

# Commit changes
git add -A
git commit -m "chore: remove deprecated components after successful modernization"
```

---

## 🔄 Rollback Plan (If Needed)

If issues arise with modern views:

1. **Restore old files from git:**
   ```bash
   git checkout HEAD~1 -- path/to/old/file.vue
   ```

2. **Update router to use old views:**
   - Revert `router/index.ts` changes
   - Revert `App.vue` changes

3. **Test thoroughly before deploying fix**

---

## ✅ Completion Criteria

Before deleting deprecated files, ensure:

- [ ] All modern views tested in production
- [ ] No user-reported issues for 2+ weeks
- [ ] Performance metrics stable or improved
- [ ] All team members trained on new components
- [ ] Documentation updated
- [ ] No remaining imports of old files
- [ ] Backup/snapshot created before deletion

---

## 📝 Notes

- Old files currently exist but are **not imported** anywhere
- Router configured to use only modern views
- App.vue uses ModernLayout exclusively
- All modern components are production-ready
- Zero compilation errors

---

**Status:** 🟡 Deprecated files identified, modern system fully operational

**Next Action:** Test modern views thoroughly, then schedule cleanup

---

*Created: November 18, 2025*
