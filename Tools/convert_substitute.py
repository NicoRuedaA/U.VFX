"""Run with Blender: blender --background --python Tools/convert_substitute.py.
Converts the bundled, attributed static glTF to Unity's native FBX import format.
"""
from pathlib import Path
import bpy
from mathutils import Vector

root = Path(__file__).resolve().parents[1]
folder = root / 'Assets/BotwVFX/Models/Substitute'
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.gltf(filepath=str(folder / 'Source/scene.gltf'))
meshes = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH']
assert len(meshes) == 2, 'Unexpected source geometry; inspect before converting'
bpy.ops.object.select_all(action='DESELECT')
for obj in meshes:
    obj.select_set(True)
bpy.context.view_layer.objects.active = meshes[0]
bpy.ops.object.join()
model = bpy.context.object
model.name = 'Substitute'
# Bake the imported glTF hierarchy before reparenting and normalizing.
world = model.matrix_world.copy()
model.parent = None
model.matrix_world = world
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
lo = Vector(tuple(min(v.co[i] for v in model.data.vertices) for i in range(3)))
hi = Vector(tuple(max(v.co[i] for v in model.data.vertices) for i in range(3)))
scale = 1.65 / (hi.z - lo.z)
center = Vector(((lo.x + hi.x) / 2, (lo.y + hi.y) / 2, lo.z))
for vertex in model.data.vertices:
    vertex.co = (vertex.co - center) * scale
# Existing Toon Lit multiplies vertex colours; explicitly supply white.
colors = model.data.color_attributes.new(name='Color', type='BYTE_COLOR', domain='CORNER')
for entry in colors.data:
    entry.color = (1, 1, 1, 1)
model.data.color_attributes.active_color = colors
bpy.ops.export_scene.fbx(filepath=str(folder / 'Substitute.fbx'), use_selection=True,
    object_types={'MESH'}, axis_forward='-Z', axis_up='Y', bake_anim=False,
    use_mesh_modifiers=True, add_leaf_bones=False, path_mode='RELATIVE')
print(f'Substitute: {len(model.data.vertices)} vertices, {len(model.data.polygons)} polygons; height 1.65m')
