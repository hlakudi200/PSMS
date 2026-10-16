import { useEffect } from "react";
import { useChildrenActions, useChildrenState } from "../../providers/children";

/**
 * The child every parent screen scopes its reads to. Any parent screen can be the first one
 * opened — Home is what normally loads the children — so this loads them if nothing has.
 */
export function useSelectedChild() {
  const { myChildren, selectedChildId, isPending, isError } = useChildrenState();
  const { getMyChildrenAsync, selectChild } = useChildrenActions();

  useEffect(() => {
    if (!myChildren && !isPending && !isError) void getMyChildrenAsync();
  }, [myChildren, isPending, isError, getMyChildrenAsync]);

  return {
    child: myChildren?.find((c) => c.studentId === selectedChildId),
    myChildren,
    selectedChildId,
    selectChild,
    hasChildren: myChildren ? myChildren.length > 0 : undefined,
    isChildrenError: isError && !myChildren,
    retryChildren: getMyChildrenAsync,
  };
}
