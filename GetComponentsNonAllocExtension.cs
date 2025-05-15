using System.Collections.Generic;
using UnityEngine;

public static class GetComponentsNonAllocExtension
{
	public static List<T> GetComponentsNonAlloc<T>(this Component comp, out ListReturnHandle<T> handle)
	{
		List<T> list = DewPool.GetList(out handle);
		list.Clear();
		comp.GetComponents(list);
		return list;
	}

	public static List<T> GetComponentsInChildrenNonAlloc<T>(this Component comp, out ListReturnHandle<T> handle)
	{
		List<T> list = DewPool.GetList(out handle);
		list.Clear();
		comp.GetComponentsInChildren(list);
		return list;
	}

	public static List<T> GetComponentsInChildrenNonAlloc<T>(this Component comp, bool includeInactive, out ListReturnHandle<T> handle)
	{
		List<T> list = DewPool.GetList(out handle);
		list.Clear();
		comp.GetComponentsInChildren(includeInactive, list);
		return list;
	}

	public static List<T> GetComponentsInParentNonAlloc<T>(this Component comp, out ListReturnHandle<T> handle)
	{
		List<T> list = DewPool.GetList(out handle);
		list.Clear();
		comp.GetComponentsInParent(includeInactive: false, list);
		return list;
	}

	public static List<T> GetComponentsInParentNonAlloc<T>(this Component comp, bool includeInactive, out ListReturnHandle<T> handle)
	{
		List<T> list = DewPool.GetList(out handle);
		list.Clear();
		comp.GetComponentsInParent(includeInactive, list);
		return list;
	}

	public static List<T> GetComponentsNonAlloc<T>(this GameObject gobj, out ListReturnHandle<T> handle)
	{
		List<T> list = DewPool.GetList(out handle);
		list.Clear();
		gobj.GetComponents(list);
		return list;
	}

	public static List<T> GetComponentsInChildrenNonAlloc<T>(this GameObject gobj, out ListReturnHandle<T> handle)
	{
		List<T> list = DewPool.GetList(out handle);
		list.Clear();
		gobj.GetComponentsInChildren(list);
		return list;
	}

	public static List<T> GetComponentsInChildrenNonAlloc<T>(this GameObject gobj, bool includeInactive, out ListReturnHandle<T> handle)
	{
		List<T> list = DewPool.GetList(out handle);
		list.Clear();
		gobj.GetComponentsInChildren(includeInactive, list);
		return list;
	}

	public static List<T> GetComponentsInParentNonAlloc<T>(this GameObject gobj, out ListReturnHandle<T> handle)
	{
		List<T> list = DewPool.GetList(out handle);
		list.Clear();
		gobj.GetComponentsInParent(includeInactive: false, list);
		return list;
	}

	public static List<T> GetComponentsInParentNonAlloc<T>(this GameObject gobj, bool includeInactive, out ListReturnHandle<T> handle)
	{
		List<T> list = DewPool.GetList(out handle);
		list.Clear();
		gobj.GetComponentsInParent(includeInactive, list);
		return list;
	}
}
