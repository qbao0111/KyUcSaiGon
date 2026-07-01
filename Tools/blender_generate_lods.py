import bpy
import json
import os
import sys


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.materials, bpy.data.images, bpy.data.armatures):
        for datablock in list(datablocks):
            if datablock.users == 0:
                datablocks.remove(datablock)


def simplify_meshes(ratio):
    before = 0
    after = 0
    for obj in list(bpy.context.scene.objects):
        if obj.type != "MESH":
            continue
        before += len(obj.data.vertices)
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        modifier = obj.modifiers.new(name="LOD_Decimate", type="DECIMATE")
        modifier.decimate_type = "COLLAPSE"
        modifier.ratio = ratio
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        after += len(obj.data.vertices)
        obj.select_set(False)
    return before, after


def export_lod(source, destination, ratio):
    clear_scene()
    bpy.ops.import_scene.gltf(filepath=source)
    before, after = simplify_meshes(ratio)
    os.makedirs(os.path.dirname(destination), exist_ok=True)
    bpy.ops.export_scene.gltf(
        filepath=destination,
        export_format="GLB",
        export_materials="NONE",
        export_animations=False,
        export_apply=True,
        export_cameras=False,
        export_lights=False,
    )
    print(f"LOD_RESULT|{os.path.basename(source)}|ratio={ratio}|before={before}|after={after}|output={destination}")


def main():
    argv = sys.argv
    if "--" not in argv:
        raise RuntimeError("Expected a JSON job file after --")
    job_path = argv[argv.index("--") + 1]
    with open(job_path, "r", encoding="utf-8") as handle:
        jobs = json.load(handle)
    for job in jobs:
        export_lod(job["source"], job["destination"], float(job["ratio"]))


if __name__ == "__main__":
    main()
